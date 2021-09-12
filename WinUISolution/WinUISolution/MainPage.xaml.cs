using System;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Storage;

public struct Half4
{
    public Fox.Half X;
    public Fox.Half Y;
    public Fox.Half Z;
    public Fox.Half W;

    public Half4(BinaryReader reader)
    {
        X = Fox.Half.ToHalf(reader.ReadUInt16());
        Y = Fox.Half.ToHalf(reader.ReadUInt16());
        Z = Fox.Half.ToHalf(reader.ReadUInt16());
        W = Fox.Half.ToHalf(reader.ReadUInt16());
    }
}

public struct HeaderMetadata0
{
    public ushort U0;
    public ushort U1;
    public uint VariableNameHash;
    public uint U2;
    public float Value;

    public HeaderMetadata0(BinaryReader reader)
    {
        U0 = reader.ReadUInt16();
        U1 = reader.ReadUInt16();
        VariableNameHash = reader.ReadUInt32();
        U2 = reader.ReadUInt32();
        Value = reader.ReadSingle();
    }
}

public struct HeaderMetadata1
{
    public ushort U0;
    public ushort U1;
    public uint VariableNameHash;
    public uint U2;
    public uint Value;

    public HeaderMetadata1(BinaryReader reader)
    {
        U0 = reader.ReadUInt16();
        U1 = reader.ReadUInt16();
        VariableNameHash = reader.ReadUInt32();
        U2 = reader.ReadUInt32();
        Value = reader.ReadUInt32();
    }
}

public struct Object
{
    public float YTranslation;
    public short xTranslation;
    public float XTranslation;
    public short zTranslation;
    public float ZTranslation;
    public Half4 RotationQuaternion;
    public ushort BlockID;
    public byte BrushID;
    public byte YScale;
    public uint ID;

    public Object(BinaryReader reader)
    {
        YTranslation = reader.ReadSingle();
        xTranslation = reader.ReadInt16();
        XTranslation = 0;
        zTranslation = reader.ReadInt16();
        ZTranslation = 0;
        RotationQuaternion = new Half4(reader);
        BlockID = reader.ReadUInt16();
        BrushID = reader.ReadByte();
        YScale = reader.ReadByte();
        ID = reader.ReadUInt32();
    }
}

namespace WinUISolution
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const float CentimetersToMeters = 1.0f / 100;
        const float MetersToCentimeters = 100;

        private static Point ConvertCoords(double x, double y)
        {
            return new Point(2048 + x, 4096 - (2048 + y));
        }

        private void CreateRectanglePoint(double x, double y)
        {
            var path = new Microsoft.UI.Xaml.Shapes.Path
            {
                Fill = new SolidColorBrush(Colors.Green),
                StrokeThickness = 1,
                Data = new EllipseGeometry
                {
                    Center = ConvertCoords(x, y),
                    RadiusX = 100 * CentimetersToMeters,
                    RadiusY = 100 * CentimetersToMeters,
                }
            };
            path.SetValue(Canvas.LeftProperty, 0);
            path.SetValue(Canvas.TopProperty, 0);
            path.SetValue(Canvas.ZIndexProperty, 1);
            OBRCanvas.Children.Add(path);
        }

        public MainPage()
        {
            this.InitializeComponent();

            var directory = Directory.GetCurrentDirectory();
            var originalBinaryPath = directory.Substring(0, directory.Length - 5) + @"\cypr_04_obr.obr";
            var originalPointsTSVPath = directory.Substring(0, directory.Length - 5) + @"\cypr_04_objBrush.tsv";

            {
                var rectangle = new Rectangle();
                rectangle.Width = 128;
                rectangle.Height = 128;
                rectangle.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 208, 208, 208));
                var center = ConvertCoords(-64 - 64, -1600 + 64);
                rectangle.SetValue(Canvas.LeftProperty, center.X);
                rectangle.SetValue(Canvas.TopProperty, center.Y);
                rectangle.SetValue(Canvas.ZIndexProperty, 0);

                OBRCanvas.Children.Add(rectangle);

                CreateRectanglePoint(-128f, -1536f);
                CreateRectanglePoint(0f, -1536f);
                CreateRectanglePoint(-128f, -1664f);
                CreateRectanglePoint(0f, -1664f);
            }

            {
                var indicatorPoint = new Microsoft.UI.Xaml.Shapes.Path
                {
                    Fill = new SolidColorBrush(Colors.Yellow),
                    StrokeThickness = 1,
                    Data = new EllipseGeometry
                    {
                        Center = ConvertCoords(-60.93f, -1667.354f),
                        RadiusX = 20 * CentimetersToMeters,
                        RadiusY = 20 * CentimetersToMeters,
                    }
                };
                indicatorPoint.SetValue(Canvas.LeftProperty, 0);
                indicatorPoint.SetValue(Canvas.TopProperty, 0);
                indicatorPoint.SetValue(Canvas.ZIndexProperty, 1);
                indicatorPoint.Opacity = 0.2;
                OBRCanvas.Children.Add(indicatorPoint);
            }

            if (File.Exists(originalBinaryPath))
                return;
            if (File.Exists(originalPointsTSVPath))
                return;

            StorageFile file = StorageFile.GetFileFromPathAsync(originalBinaryPath).GetAwaiter().GetResult();
            StorageFile xmlFile = StorageFile.GetFileFromPathAsync(originalPointsTSVPath).GetAwaiter().GetResult();
            using (var reader = new BinaryReader(file.OpenStreamForReadAsync().Result))
            using (var streamReader = new StreamReader(xmlFile.OpenStreamForReadAsync().Result))
            {
                reader.BaseStream.Position = 4 * 11;
                uint numBlocksHOffset = reader.ReadUInt32();

                reader.BaseStream.Position += 4 * 12;

                HeaderMetadata0 blockSizeWMetadata = new HeaderMetadata0(reader);
                HeaderMetadata0 blockSizeHMetadata = new HeaderMetadata0(reader);
                HeaderMetadata1 numBlocksWMetadata = new HeaderMetadata1(reader);
                HeaderMetadata1 numBlocksHMetadata = new HeaderMetadata1(reader);
                HeaderMetadata1 numObjectsMetadata = new HeaderMetadata1(reader);

                reader.BaseStream.Position += 80;

                Object[] objects = new Object[numObjectsMetadata.Value];
                for (int i = 0; i < numObjectsMetadata.Value; i++)
                {
                    ref Object obj = ref objects[i];
                    obj = new Object(reader);

                    obj.XTranslation = blockSizeWMetadata.Value * (obj.BlockID % numBlocksWMetadata.Value + 0.5f - 0.5f * numBlocksWMetadata.Value) + blockSizeWMetadata.Value * obj.xTranslation / short.MaxValue;
                    obj.ZTranslation = blockSizeHMetadata.Value * (obj.BlockID / numBlocksHMetadata.Value + 0.5f - 0.5f * numBlocksHMetadata.Value) + blockSizeHMetadata.Value * obj.zTranslation / short.MaxValue;

                    var pointString = streamReader.ReadLine();
                    var parsedPoint = pointString.Split('\t');
                    var xmlPoint = new Point(double.Parse(parsedPoint[0]), double.Parse(parsedPoint[1]));
                    AddPoint(xmlPoint, new Point(obj.XTranslation, obj.ZTranslation));
                }
            }
        }

        private void AddPoint(Point xml, Point obr)
        {
            // OBR Point
            {
                var path = new Microsoft.UI.Xaml.Shapes.Path
                {
                    Fill = new SolidColorBrush(((obr.X == 786) && (obr.Y == 15525)) ? Colors.Purple : Colors.Red),
                    StrokeThickness = 1,
                    Data = new EllipseGeometry
                    {
                        Center = ConvertCoords(obr.X, obr.Y),
                        RadiusX = 0.01,
                        RadiusY = 0.01,
                    }
                };
                path.SetValue(Canvas.LeftProperty, 0);
                path.SetValue(Canvas.TopProperty, 0);
                path.SetValue(Canvas.ZIndexProperty, 2);
                OBRCanvas.Children.Add(path);
            }

            // XML Point
            {
                var path = new Microsoft.UI.Xaml.Shapes.Path
                {
                    Fill = new SolidColorBrush(((xml.X == 786) && (xml.Y == 15525)) ? Colors.Orange : Colors.Blue),
                    StrokeThickness = 1,
                    Data = new EllipseGeometry
                    {
                        Center = ConvertCoords(xml.X, xml.Y),
                        RadiusX = 0.01,
                        RadiusY = 0.01,
                    }
                };
                path.SetValue(Canvas.LeftProperty, 0);
                path.SetValue(Canvas.TopProperty, 0);
                path.SetValue(Canvas.ZIndexProperty, 2);
                OBRCanvas.Children.Add(path);
            }

            // ConnectingLine
            {
                var delta = new Point(obr.X - xml.X, obr.Y - xml.Y);
                var length = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

                if (length > 50 * CentimetersToMeters)
                    return;

                var obrConv = ConvertCoords(obr.X, obr.Y);
                var xmlConv = ConvertCoords(xml.X, xml.Y);
                var line = new Line
                {
                    X1 = obrConv.X,
                    Y1 = obrConv.Y,
                    X2 = xmlConv.X,
                    Y2 = xmlConv.Y,
                    Stroke = new SolidColorBrush(Colors.Green),
                    StrokeThickness = 0.005,
                };
                line.SetValue(Canvas.LeftProperty, 0);
                line.SetValue(Canvas.TopProperty, 0);
                line.SetValue(Canvas.ZIndexProperty, 4);
                OBRCanvas.Children.Add(line);
            }
        }
    }
}
