using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public byte[,] img;
        WriteableBitmap bm;
        //string fname;
        public int img_scale;
        public ushort[,] image_base;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            return;
        }

         // Load an image from disk for ROI drawing
        public void load_button_Click(object sender, RoutedEventArgs e)
        {

            var fname = "C:/Users/BehaveRig/Desktop/TM00000_CM0_CHN00-0083.tif";
            var img = load_tif(fname, 0);
            image_base = new ushort[img.GetLength(0), img.GetLength(1)]; 
            image_base = (ushort[,])img.Clone();
            
            var w = img.GetLength(1);
            var h = img.GetLength(0);
                              
            bm = new WriteableBitmap(w, h, 96, 96, PixelFormats.Gray16, BitmapPalettes.Gray256);
            var rect = new System.Windows.Int32Rect(0, 0, w, h);
            var stride = w * 2;    
            bm.WritePixels(rect, img, stride, 0);
            image1.Width = w / 3;
            image1.Height = h / 3; 
            image1.Source = bm;            
        }


        // Only works for 16 bit images
        private ushort[,] load_tif(string Path, int Plane)
        {
 
            var image = Tiff.Open(Path, "r");
            //var endian = image.IsBigEndian();

            FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = image.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();
            
            value = image.GetField(TiffTag.BITSPERSAMPLE);
            int bytes_per_pixel = value[0].ToInt() / 8;

            ushort[,] img = new ushort[width, height];            

            byte[][] buf = new byte[height][];
            for (int row = 0; row < height; row++)
            {
                buf[row] = new byte[image.ScanlineSize()];
                image.ReadScanline(buf[row], row);
            }
                       
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    {
                        var x_ = bytes_per_pixel * x;                       
                        img[x, y] = (ushort)BitConverter.ToUInt16(buf[y], x_);                      
                    }
                }

            image.Close();                        
            return img;
        }

      
        private ushort[] Rescale_Array_1D(ushort[] input, double scale)
        {
            ushort[] output = new ushort[input.Length];

            for (int x = 0; x < input.Length; x++)
            {
                var new_val = (double)input[x] * scale;
                var upper = (double)Math.Pow(2, 16) - 1; 
                
                if (new_val >= upper)
                { new_val = upper; }

                else if (new_val <= 0.0)
                {new_val = upper;}                    
                
                output[x] = (ushort)new_val;
            }
            return output;
        }


        private short[,] Rescale_Array_2D(short[,] input, double scale)
        {
                short[,] output = new short[input.GetLength(0), input.GetLength(1)];

                for (int x = 0; x < input.GetLength(0); x++)
                {
                    for (int y = 0; y < input.GetLength(1); y++)
                    {
                        output[x, y] = (short)(input[x, y] * scale);
                    }
                }
                return output;
        }
       

        private void imadjust_slider_1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (image1.Source != null)
            {
            var slider = (Slider)sender;            
            WriteableBitmap bm = (WriteableBitmap)image1.Source;
            var w = bm.PixelWidth;
            var h = bm.PixelHeight;
            ushort[] new_img = new ushort[h*w];
            
            for (int x = 0; x < image_base.GetLength(0); x++)
            {
                for (int y = 0; y < image_base.GetLength(1); y++)
                {
                    new_img[x * w + y] = image_base[x, y];
                }
            }
            //bm.CopyPixels(new_img, w * 2, 0);
            var stride = w * 2;
            bm.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), Rescale_Array_1D(new_img, slider.Value), stride, 0);
            }
        }

    }
}
