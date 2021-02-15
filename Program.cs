using System;
using System.IO;

namespace OBRVisualizer
{
    class Program
    {
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

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            using (var reader = new BinaryReader(new FileStream(args[0], FileMode.Open)))
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

                    obj.XTranslation = blockSizeWMetadata.Value * (obj.BlockID % numBlocksWMetadata.Value + 0.5f - 0.5f*numBlocksWMetadata.Value) + blockSizeWMetadata.Value * obj.xTranslation / short.MaxValue;
                    obj.ZTranslation = blockSizeHMetadata.Value * (obj.BlockID / numBlocksHMetadata.Value + 0.5f - 0.5f*numBlocksHMetadata.Value) + blockSizeHMetadata.Value * obj.zTranslation / short.MaxValue;

                    Console.WriteLine($"Object: (ID = {obj.ID}, blockID = {obj.BlockID}, quat = (X = {obj.RotationQuaternion.X}, Y = {obj.RotationQuaternion.Y}, Z = {obj.RotationQuaternion.Z}, W = {obj.RotationQuaternion.W}), Pos = (X = {obj.XTranslation}, Y = {obj.YTranslation}, Z = {obj.ZTranslation})");
                }

                Console.WriteLine("Done!");
                Console.ReadKey();
            }
        }
    }
}
