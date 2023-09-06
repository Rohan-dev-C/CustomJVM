using System;
using System.IO;
using System.Collections.Generic; 

namespace CustomJVM
{
    class Program
    {
        static void ParseMagic(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
            => classFile.magic = bytecode.U4();
        static void ParseVersion(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            classFile.MinorVersion = bytecode.U2();
            classFile.MajorVersion = bytecode.U2(); 
               
        }
        static void ParseCPCount(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
            => classFile.ConstantPoolCount = bytecode.U2(); 
        private static void ParseConstantPool(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            classFile.CP_Infos = new CP_Info[classFile.ConstantPoolCount - 1];
            for (int i = 0; i < classFile.CP_Infos.Length; i++)
            {
                CP_Info info = default;
                var tag = (CP_Info.Tags)bytecode.U1();
                switch (tag)
                {
                    case CP_Info.Tags.CONSTANT_Class:
                        info = new CONSTANT_Class_info(); 
                        break;
                    case CP_Info.Tags.CONSTANT_Fieldref:
                        info = new CONSTANT_Fieldref_info(); 
                        break; 

                    case CP_Info.Tags.CONSTANT_Methodref:
                        info = new CONSTANT_MethodRef_info(); 
                        break;

                    case CP_Info.Tags.CONSTANT_InterfaceMethodref:
                        info = new CONSTANT_InterfaceMethodref_info(); 
                        break;

                    case CP_Info.Tags.CONSTANT_String:
                        info = new CONSTANT_String_info(); 
                        break;

                    case CP_Info.Tags.CONSTANT_Integer:
                        info = new CONSTANT_Integer_info(); 
                        break;

                    case CP_Info.Tags.CONSTANT_Float:
                        info = new CONSTANT_Float_info();

                        break;

                    case CP_Info.Tags.CONSTANT_Long:
                        info = new CONSTANT_Long_info();
                        break;

                    case CP_Info.Tags.CONSTANT_Double:
                        info = new CONSTANT_Double_info();
                        break;

                    case CP_Info.Tags.CONSTANT_NameAndType:
                        info = new CONSTANT_NameAndType_info();
                        break;

                    case CP_Info.Tags.CONSTANT_Utf8:
                        info = new CONSTANT_Utf8_info();
                        break;

                    case CP_Info.Tags.CONSTANT_MethodHandle:
                        info = new CONSTANT_MethodHandle_info(); 
                        break;

                    case CP_Info.Tags.CONSTANT_MethodType:
                        info = new CONSTANT_MethodType_info(); 
                        break;

                    case CP_Info.Tags.CONSTANT_InvokeDynamic:
                        info = new CONSTANT_InvokeDynamic_info(); 
                        break;
                    default: throw new Exception("Uknown CONSTANT POOL TYPE");
                }
                info.Parse(classFile, ref bytecode);
                classFile.CP_Infos[i] = info; 
            }
        }
        static void ParseAccessFlags(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
            => classFile.access_flags = bytecode.U2();
        static void ParseThisClass(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
          => classFile.this_class = bytecode.U2();
        static void ParseSuperClass(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
          => classFile.super_class = bytecode.U2();
        static void ParseInterfacesCount(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
          => classFile.interfaces_count = bytecode.U2();
        private static void ParseInterfaces(ClassFile classFile, ref ReadOnlySpan<byte> bytecode) { }
        static void ParseFieldsCount(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
          => classFile.interfaces_count = bytecode.U2();
        private static void ParseFields(ClassFile classFile, ref ReadOnlySpan<byte> bytecode) { }
        static void ParseMethodsCount(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
          => classFile.methods_Count = bytecode.U2();
        private static void ParseMethods(ClassFile classFile, ref ReadOnlySpan<byte> bytecode) {
            classFile.Methods_Infos = new Methods_Info[classFile.methods_Count];
            for (int i = 0; i < classFile.Methods_Infos.Length; i++)
            {
                classFile.Methods_Infos[i] = new Methods_Info();
                classFile.Methods_Infos[i].Parse(classFile, ref bytecode);
            }

        }
        static void ParseAttributesCount(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
           => classFile.attributes_Count = bytecode.U2();
        private static void ParseAttributes(ClassFile classFile, ref ReadOnlySpan<byte> bytecode) 
        {
            classFile.attributes_Infos = new Attributes_Info[classFile.attributes_Count];
            for (int i = 0; i < classFile.attributes_Infos.Length; i++)
            {
                var ushortNameIndex = bytecode.U2();
                var temp = classFile.CP_Infos[ushortNameIndex - 1].ToString();
                var attributeType = Enum.Parse<Attributes_Info.Attributes>(temp);
                switch (attributeType)
                {
                    case Attributes_Info.Attributes.ConstantValue:
                        break;
                    case Attributes_Info.Attributes.Code:
                        classFile.attributes_Infos[i] = new Code();
                        classFile.attributes_Infos[i].attribute_name_index = ushortNameIndex;
                        classFile.attributes_Infos[i].attribute_length = bytecode.U4();
                        classFile.attributes_Infos[i].Parse(classFile, ref bytecode);
                        break;
                    case Attributes_Info.Attributes.StackMapTable:
                        break;
                    case Attributes_Info.Attributes.Exceptions:
                        break;
                    case Attributes_Info.Attributes.InnerClasses:
                        break;
                    case Attributes_Info.Attributes.EnclosingMethod:
                        break;
                    case Attributes_Info.Attributes.Synthetic:
                        break;
                    case Attributes_Info.Attributes.Signature:
                        break;
                    case Attributes_Info.Attributes.SourceFile:

                        classFile.attributes_Infos[i] = new SourceFile();
                        classFile.attributes_Infos[i].attribute_name_index = ushortNameIndex;
                        classFile.attributes_Infos[i].attribute_length = bytecode.U4();
                        classFile.attributes_Infos[i].Parse(classFile, ref bytecode);
                        break;
                    case Attributes_Info.Attributes.SourceDebugExtension:
                        break;
                    case Attributes_Info.Attributes.LineNumberTable:

                        classFile.attributes_Infos[i] = new LineNumberTable();
                        classFile.attributes_Infos[i].attribute_name_index = ushortNameIndex;
                        classFile.attributes_Infos[i].attribute_length = bytecode.U4();
                        classFile.attributes_Infos[i].Parse(classFile, ref bytecode);
                        break;
                    case Attributes_Info.Attributes.LocalVariableTable:
                        break;
                    case Attributes_Info.Attributes.LocalVariableTypeTable:
                        break;
                    case Attributes_Info.Attributes.Deprecated:
                        break;
                    case Attributes_Info.Attributes.RuntimeVisibleAnnotations:
                        break;
                    case Attributes_Info.Attributes.RuntimeInvisibleAnnotations:
                        break;
                    case Attributes_Info.Attributes.RuntimeVisibleParameterAnnotations:
                        break;
                    case Attributes_Info.Attributes.RuntimeInvisibleParameterAnnotations:
                        break;
                    case Attributes_Info.Attributes.AnnotationDefault:
                        break;
                    case Attributes_Info.Attributes.BootstrapMethods:
                        break;
                    default:
                        break;
                }
            }
        }
        static void Main(string[] args)
        {
            CONSTANT_MethodRef_info methodRef_Info = new CONSTANT_MethodRef_info();
            Stack<int> stack = new Stack<int>(); 
            var bytes = File.ReadAllBytes(args[0]);
            ReadOnlySpan<byte> byteCode = bytes.AsSpan<byte>();
            ClassFile classFile = new ClassFile();
            int methodIndex = 0; 
            ParseMagic(classFile, ref byteCode);
            ParseVersion(classFile, ref byteCode);
            ParseCPCount(classFile, ref byteCode);
            ParseConstantPool(classFile, ref byteCode);
            ParseAccessFlags(classFile, ref byteCode);
            ParseThisClass(classFile, ref byteCode);
            ParseSuperClass(classFile, ref byteCode);
            ParseInterfacesCount(classFile, ref byteCode);
            ParseInterfaces(classFile, ref byteCode);
            ParseFieldsCount(classFile, ref byteCode);
            ParseFields(classFile, ref byteCode);
            ParseMethodsCount(classFile, ref byteCode);
            ParseMethods(classFile, ref byteCode);
            ParseAttributesCount(classFile, ref byteCode);
            ParseAttributes(classFile, ref byteCode);
            var temp = classFile.SearchForMain(classFile);
            classFile.ExcecuteMethodCode(temp, ref stack); 
            Console.WriteLine($"0x{classFile.magic:X8}");
            Console.WriteLine($"0x{classFile.MinorVersion:X8}");
            Console.WriteLine($"0x{classFile.MajorVersion:X8}");
            Console.WriteLine($"0x{classFile.ConstantPoolCount:X8}");

        }
    }
}
