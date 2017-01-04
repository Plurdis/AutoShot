﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using f=System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoCapturer.UserControls
{
    /// <summary>
    /// ImageEdit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageEdit : System.Windows.Controls.UserControl
    {
        // 외부 공개용 속성

        public Stretch ImageStretchMode
        {
            get { return InnerImg.Stretch; }
            set { InnerImg.Stretch = value; }
        }

        private EditMode _ImageEditMode;
        public EditMode ImageEditMode
        {
            get { return _ImageEditMode; }
            set
            {
                _ImageEditMode = value;
                switch (_ImageEditMode)
                {
                    case EditMode.SizeChange:
                        RC_Dragger.Visibility = Visibility.Visible;
                        RUC_Dragger.Visibility = Visibility.Visible;
                        RD_Dragger.Visibility = Visibility.Visible;
                        CropGrid.Visibility = Visibility.Hidden;
                        break;
                    case EditMode.CropImage:
                        RC_Dragger.Visibility = Visibility.Hidden;
                        RUC_Dragger.Visibility = Visibility.Hidden;
                        RD_Dragger.Visibility = Visibility.Hidden;
                        CropGrid.Visibility = Visibility.Visible;
                        break;
                }
            }

        }

        public struct WIN32POINT
        {
            public int X;
            public int Y;
        }


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out WIN32POINT lpPoint);



        float Ratio = 1.0F;

        public ImageEdit()
        {
            InitializeComponent();
            

            Rectangle[] CropRects = { UL_Cropper, UC_Cropper, UR_Cropper,
                                     CL_Cropper, CR_Cropper,
                                     DL_Cropper, DC_Cropper, DR_Cropper};



            foreach (Rectangle CropRect in CropRects)
            {
                CropRect.MouseDown += Cropper_Process;
            }

            foreach (Rectangle CropRect in CropRects)
            {
                CropRect.MouseUp += CropperEnd_Process;
            }


            Rectangle[] DragRects = { RC_Dragger, RUC_Dragger, RD_Dragger };


            foreach (Rectangle DragRect in DragRects)
            {
                DragRect.MouseDown += Dragger_Process;
            }

            foreach (Rectangle DragRect in DragRects)
            {
                DragRect.MouseUp += DraggerEnd_Process;
            }
            
            Dragtmr.Interval = 10;

            BitmapImage img = new BitmapImage(new Uri(@"pack://application:,,,/AutoCapturer;component/Resources/Icons/CloseImg.png"));

            CroppedBitmap CropImg = new CroppedBitmap(img, new Int32Rect(20, 20, 30, 30));

            InnerImg.Source = CropImg;
        }


        private void Tick_SizeChange(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Control.MouseButtons == f.MouseButtons.Left)
            {
                WIN32POINT NowPosition;
                GetCursorPos(out NowPosition);
                double Width, Height;
                Width = BoardSize.Width - (StartPoint.X - NowPosition.X);
                Height = BoardSize.Height - (StartPoint.Y - NowPosition.Y);
                if (Width < 11) Width = 11; if (Height < 11) Height = 11;
                if (Mode == DragMode.Both || Mode == DragMode.OnlyWidth)
                {
                    PreviewRect.Width = Width - 10;
                    ReservePoint.Width = Width;
                }
                if (Mode == DragMode.Both || Mode == DragMode.OnlyHeight)
                {
                    PreviewRect.Height = Height - 10;
                    ReservePoint.Height = Height;
                }
            }
            else
            {
                PreviewRect.Opacity = 0.0;
                if (Mode == DragMode.Both || Mode == DragMode.OnlyWidth) { MainGrid.Width = ReservePoint.Width; }
                if (Mode == DragMode.Both || Mode == DragMode.OnlyHeight){MainGrid.Height = ReservePoint.Height; }

                

                this.Cursor = null;
                Dragtmr.Stop();
            }
        }

        private void DraggerEnd_Process(object sender, MouseButtonEventArgs e)
        {
            Mode = DragMode.Wait;
            StartPoint.X = 0; StartPoint.Y = 0;
            this.Cursor = null;
            Dragtmr.Stop();
        }

        private void Dragger_Process(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = ((Rectangle)sender).Cursor;
            GetCursorPos(out StartPoint);
            BoardSize = new Size(MainGrid.Width, MainGrid.Height);
            PreviewRect.Opacity = 1.0;
            switch ((string)((Rectangle)sender).Tag)
            {
                case "Width":
                    Mode = DragMode.OnlyWidth;
                    break;
                case "Height":
                    Mode = DragMode.OnlyHeight;
                    break;
                case "Both":
                    Mode = DragMode.Both;
                    break;
            }
            Dragtmr.Tick += Tick_SizeChange;
            Dragtmr.Start();
        }

        private void Cropper_Process(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = ((Rectangle)sender).Cursor;
            GetCursorPos(out StartPoint);
            BoardSize = new Size(MainGrid.Width, MainGrid.Height);


            switch (((Rectangle)sender).Name.Substring(0, 2))
            {
                case "UL":
                    IncMode = IncreaseMode.RightIncrease | IncreaseMode.UpIncrease;
                    break;
                case "UC":
                    IncMode = IncreaseMode.UpIncrease;
                    break;
                case "UR":
                    IncMode = IncreaseMode.LeftIncrease | IncreaseMode.UpIncrease;
                    break;
                case "CL":
                    IncMode = IncreaseMode.RightIncrease;
                    break;
                case "CR":
                    IncMode = IncreaseMode.LeftIncrease;
                    break;
                case "DL":
                    IncMode = IncreaseMode.LeftIncrease | IncreaseMode.DownIncrease;
                    break;
                case "DC":
                    IncMode = IncreaseMode.DownIncrease;
                    break;
                case "DR":
                    IncMode = IncreaseMode.RightIncrease | IncreaseMode.DownIncrease;
                    break;
            }
        }
        private void CropperEnd_Process(object sender, MouseButtonEventArgs e)
        {

        }


        Size BoardSize;
        Size ReservePoint;

        WIN32POINT StartPoint;
        DragMode Mode = DragMode.Wait;
        IncreaseMode IncMode = IncreaseMode.LeftIncrease;

        f.Timer Dragtmr = new f.Timer();
        f.Timer Croptmr = new f.Timer();

        


        

    }
    public enum DragMode
    {
        OnlyWidth, OnlyHeight, Both, Wait
    }

    [Flags]
    public enum IncreaseMode
    {
        None = 0,
        LeftIncrease = 1,
        RightIncrease = 2,
        DownIncrease = 3,
        UpIncrease = 4
    }

    public enum EditMode
    {
        SizeChange, Select, CropImage
    }
}
