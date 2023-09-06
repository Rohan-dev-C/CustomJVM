using System;
using System.Collections.Generic;
using System.Text;

namespace CustomJVM
{
    public static class Extensions {
        public static byte U1(ref this ReadOnlySpan<byte> bytes)
        {
            byte returnME = bytes[0];
            bytes = bytes.Slice(1);
            return returnME;
        }
        public static ushort U2(ref this ReadOnlySpan<byte> bytes) => (ushort)(bytes.U1() << 8 | bytes.U1());

        public static uint U4(ref this ReadOnlySpan<byte> bytes) => (uint)(bytes.U2() << 16 | bytes.U2());
    }
    class ClassFile
    {
        

        public uint magic { get; set; }
        public ushort MinorVersion { get; set; }
        public ushort MajorVersion { get; set; }
        public ushort ConstantPoolCount { get; set; }
        public CP_Info[] CP_Infos { get; set; }
        public ushort access_flags { get; set; }
        public ushort this_class { get; set; }
        public ushort super_class { get; set; }
        public ushort interfaces_count { get; set; }
        public Interfaces_Info[] interfaces_Infos { get; set; }
        public ushort fields_count { get; set; }
        public Fields_Info[] fields_Infos { get; set; }
        public ushort methods_Count { get; set; }
        public Methods_Info[] Methods_Infos { get; set; }
        public uint attributes_Count { get; set; }
        public Attributes_Info[] attributes_Infos { get; set; }

        public Methods_Info SearchForMain(ClassFile classFile)
        {
            for (int i = 0; i < Methods_Infos.Length; i++)
            {
                Methods_Info currentMethodInfo = classFile.Methods_Infos[i]; ;
                if(currentMethodInfo.access_flag != null)
                {
                    if(currentMethodInfo.access_flag.HasFlag(Methods_Info.AccessFlags.ACC_PUBLIC) && currentMethodInfo.access_flag.HasFlag(Methods_Info.AccessFlags.ACC_STATIC))
                    {
                        if (CP_Infos[currentMethodInfo.descriptor_index -1].ToString() == "([Ljava/lang/String;)V")
                        {
                            return currentMethodInfo ; 
                        }
                    }
                }
            }
            return null; 
        }
   
        public void ExcecuteMethodCode(Methods_Info methodInfo, ref Stack<int> stack)
        {
            Code code = new Code();
            int maxIndex = 0;
            for (int i = 0; i < methodInfo.attributes.Length; i++)
            {
                if(methodInfo.attributes[i].attribute_name_index == 6)
                {
                    code = (Code)methodInfo.attributes[i];
                }
            }
            int[] locals = new int[code.max_locals];
            ReadOnlySpan<byte> bytes = code.code;
            while(bytes.Length > 0)
            {
                var OpCode = bytes.U1(); 
                switch (OpCode)
                {
                    case 0x10:
                        stack.Push(bytes.U1()); 
                        break;

                    case 0x3c:
                        locals[1] = stack.Pop();
                        break;  

                    case 0x3d:
                        locals[2] = stack.Pop();
                        break;
                    case 0x3e:
                        locals[3] = stack.Pop(); 
                        break;
                    case 0x1b:
                        stack.Push(locals[1]); 
                        break;
                    case 0x1c:
                        stack.Push(locals[2]);
                        break;
                    case 0x06:
                        stack.Push(3); 
                        break;
                    case 0x60:
                        stack.Push(stack.Pop() + stack.Pop());
                        break;
                    case 0xb1:
                        return;
                    default:
                        throw new Exception("invalid"); 
                        break;
                
                
                
                }
                
            }
        }
    }
    #region Constant Pool
    abstract class CP_Info
    {
        public enum Tags

        {

            CONSTANT_Class = 7,
            CONSTANT_Fieldref = 9,
            CONSTANT_Methodref = 10,
            CONSTANT_InterfaceMethodref = 11,
            CONSTANT_String	= 8,
            CONSTANT_Integer = 3,
            CONSTANT_Float	= 4,
            CONSTANT_Long	= 5,
            CONSTANT_Double	= 6,
            CONSTANT_NameAndType = 12,
            CONSTANT_Utf8 = 1,
            CONSTANT_MethodHandle = 15,
            CONSTANT_MethodType	= 16,
            CONSTANT_InvokeDynamic = 18,
        }
        public Tags Tag { get; set; }
        public abstract void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode);
    }
    class CONSTANT_Class_info : CP_Info
    {
        ushort name_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            
            name_index = bytecode.U2(); 
        }

    }
    class CONSTANT_Fieldref_info : CP_Info
    {
        ushort class_index;
        ushort name_and_type_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {   class_index = bytecode.U2();
            name_and_type_index = bytecode.U2();
        }
    }
    class CONSTANT_MethodRef_info : CP_Info
    {
        ushort class_index;
        ushort name_and_type_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            class_index = bytecode.U2();
            name_and_type_index = bytecode.U2();
        }
    }
    class CONSTANT_InterfaceMethodref_info : CP_Info 
    {
        ushort class_index;
        ushort name_and_type_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            class_index = bytecode.U2();
            name_and_type_index = bytecode.U2(); 
        }

    }
    class CONSTANT_String_info : CP_Info
    {
        ushort string_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {   string_index = bytecode.U2();
        }
    }
    class CONSTANT_Integer_info : CP_Info
    {
        uint bytes;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            bytes = bytecode.U4();
        }
    }
    class CONSTANT_Float_info : CP_Info
    {
        float bytes;        
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            bytes = bytecode.U4();
        }
   
    }
    class CONSTANT_Long_info : CP_Info
    {
        uint high_bytes;
        uint low_bytes;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            high_bytes = bytecode.U4();
            low_bytes = bytecode.U4();
        }
    }
    class CONSTANT_Double_info : CP_Info 
    {
        uint high_bytes;
        uint low_bytes;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            high_bytes = bytecode.U4();
            low_bytes = bytecode.U4();
        }
    }
    class CONSTANT_NameAndType_info : CP_Info
    {   ushort name_index;
        ushort descriptor_index;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            name_index = bytecode.U2();
            descriptor_index = bytecode.U2();
        }
    }
    class CONSTANT_MethodHandle_info : CP_Info
    {
        byte reference_kind;
        ushort reference_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            reference_kind = bytecode.U1();
            reference_index = bytecode.U2();
        }
    }
    class CONSTANT_MethodType_info : CP_Info
    {
        ushort descriptor_index;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            descriptor_index = bytecode.U2();
        }
    }
    class CONSTANT_InvokeDynamic_info : CP_Info
    {
        ushort bootstrap_method_attr_index;
        ushort name_and_type_index;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> bytecode)
        {
            bootstrap_method_attr_index = bytecode.U2();
            name_and_type_index = bytecode.U2();
        }
    }
    class CONSTANT_Utf8_info : CP_Info
    {
            ushort length;
            byte[] bytes;
            public override void Parse(ClassFile classfile, ref ReadOnlySpan<byte> bytecode)
            {
                 length = bytecode.U2();
                 bytes = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = bytecode.U1(); 
                }
                 
            }

        public override string ToString()
              => Encoding.UTF8.GetString(bytes);
    }

    #endregion Constant Pool    
    #region Attributes, Interfaces, Fields, Methods
    class Interfaces_Info { }
    class Fields_Info 
    {
        //ushort access_flags;
        //ushort name_index;
        //ushort descriptor_index;
        //ushort attributes_count;
        //Attributes_Info[] attributes;



    }
    class Methods_Info 
    {
        [Flags]
        public enum AccessFlags
        { 
            ACC_PUBLIC = 0x001,
            ACC_PRIVATE = 0x0002,
            ACC_PROTECTED = 0x0004, 
            ACC_STATIC = 0x0008,
            ACC_FINAL = 0x0010,       
            ACC_SYNCHRONIZED = 0x0020,
            ACC_BRIDGE = 0x0040, 
            ACC_VARARGS = 0x0080, 
            ACC_NATIVE = 0x0100,
            ACC_ABSTRACT = 0x0400,
            ACC_STRICT = 0x0800,
            ACC_SYNTHETIC = 0x1000
        }
        public AccessFlags access_flag { get; set;  }
        public ushort name_index;
        public ushort descriptor_index;
        public ushort attributes_count;
        public Attributes_Info[] attributes; 
        public void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode) 
        {
            access_flag = (AccessFlags)byteCode.U2();
            name_index = byteCode.U2();
            descriptor_index = byteCode.U2();
            attributes_count = byteCode.U2();
            attributes = new Attributes_Info[attributes_count];
            for (int i = 0; i < attributes.Length; i++)
            {
                var ushortNameIndex = byteCode.U2(); 
                var temp = classFile.CP_Infos[ushortNameIndex-1].ToString();
                var attributeType = Enum.Parse<Attributes_Info.Attributes>(temp);
                switch (attributeType)
                {
                    case Attributes_Info.Attributes.ConstantValue:
                        attributes[i] = new ConstantValue();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
                        break;
                    case Attributes_Info.Attributes.Code:
                        attributes[i] = new Code();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
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
                        attributes[i] = new SourceFile();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
                        break;
                    case Attributes_Info.Attributes.SourceDebugExtension:
                        break;
                    case Attributes_Info.Attributes.LineNumberTable:
                        attributes[i] = new LineNumberTable();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
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
    }
    abstract class Attributes_Info 
    {
        public enum Attributes
        {
            ConstantValue,
            Code,
            StackMapTable,
            Exceptions,
            InnerClasses,
            EnclosingMethod,
            Synthetic,
            Signature,
            SourceFile,
            SourceDebugExtension,
            LineNumberTable,
            LocalVariableTable,
            LocalVariableTypeTable,
            Deprecated,
            RuntimeVisibleAnnotations,
            RuntimeInvisibleAnnotations,
            RuntimeVisibleParameterAnnotations,
            RuntimeInvisibleParameterAnnotations,
            AnnotationDefault,
            BootstrapMethods,
        }

        public Attributes index { get; set; }
        public ushort attribute_name_index;
        public uint attribute_length; 
        public abstract void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode);
    }

    class ConstantValue : Attributes_Info
    {
        ushort constantvalue_index;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode)
        {
            constantvalue_index = byteCode.U2(); 
        }
    }
    class Exceptions : Attributes_Info
    {
        ushort number_of_exceptions;
        ushort[] exception_index_table;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode)
        {
            number_of_exceptions = byteCode.U2();
            exception_index_table = new ushort[number_of_exceptions];
            for (int i = 0; i < exception_index_table.Length - 1; i++)
            {
                exception_index_table[i] = byteCode.U2();

            }
        }
    }
    class SourceFile  : Attributes_Info
    {
  
        ushort sourcefile_index;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode)
        {
            sourcefile_index = byteCode.U2(); 
        }

    }
    class Code : Attributes_Info
    {
    
        public ushort max_stack;
        public ushort max_locals;
        public uint code_length;
        public byte[] code;
        public ushort exception_table_length;
        public struct exception_table
        {  
            public ushort start_pc;
            public ushort end_pc;
            public ushort handler_pc;
            public ushort catch_type;
        }
        public exception_table[] extension_Tables;
        public ushort attributes_count;
        public Attributes_Info[] attributes;

        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode)
        {
            max_stack = byteCode.U2(); 
            max_locals = byteCode.U2();
            code_length = byteCode.U4();
            code = new byte[code_length];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = byteCode.U1();
            }
            exception_table_length = byteCode.U2();
            extension_Tables = new exception_table[exception_table_length];
            for (int i = 0; i < extension_Tables.Length; i++)
            {
                extension_Tables[i].start_pc = byteCode.U2();
                extension_Tables[i].end_pc = byteCode.U2();
                extension_Tables[i].handler_pc = byteCode.U2();
                extension_Tables[i].catch_type = byteCode.U2();
            }
            attributes_count = byteCode.U2();
            attributes = new Attributes_Info[attributes_count];
            for (int i = 0; i < attributes.Length; i++)
            {
                var ushortNameIndex = byteCode.U2();
                var temp = classFile.CP_Infos[ushortNameIndex - 1].ToString();
                var attributeType = Enum.Parse<Attributes_Info.Attributes>(temp);
                switch (attributeType)
                {
                    case Attributes_Info.Attributes.ConstantValue:
                        break;
                    case Attributes_Info.Attributes.Code:
                        attributes[i] = new Code();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
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
                        attributes[i] = new SourceFile();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
                        break;
                    case Attributes_Info.Attributes.SourceDebugExtension:
                        break;
                    case Attributes_Info.Attributes.LineNumberTable:
                        attributes[i] = new LineNumberTable();
                        attributes[i].attribute_name_index = ushortNameIndex;
                        attributes[i].attribute_length = byteCode.U4();
                        attributes[i].Parse(classFile, ref byteCode);
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

    }
    class LineNumberTable : Attributes_Info
    {
        ushort line_number_table_length;
        struct Line_Number_Table
        {   public ushort start_pc;
            public ushort line_number;
        }
        Line_Number_Table[] tables;
        public override void Parse(ClassFile classFile, ref ReadOnlySpan<byte> byteCode)
        {
            line_number_table_length = byteCode.U2();
            tables = new Line_Number_Table[line_number_table_length];
            for (int i = 0; i < tables.Length; i++)
            {
                tables[i].start_pc = byteCode.U2();
                tables[i].line_number = byteCode.U2(); 

            }
        }


    }

    #endregion

}
