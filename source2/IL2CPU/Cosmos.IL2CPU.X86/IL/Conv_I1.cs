using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I1 )]
    public class Conv_I1 : ILOp
    {
        public Conv_I1( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Peek();
            if (xSource.IsFloat)
            {
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }
            Assembler.Stack.Pop();
            switch( xSource.Size )
            {
                case 1:
                    break;
                case 2:
                case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.SignExtendAX { Size = 8 };
                    new CPUx86.SignExtendAX { Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };

                    break;
                case 8:
                    Assembler.Stack.Pop();
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                    new CPUx86.SignExtendAX { Size = 8 };
                    new CPUx86.SignExtendAX { Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException(); 
            }
            Assembler.Stack.Push(1, true, false, true);
        }


        // using System;
        // using System.IO;
        // using CPU = Cosmos.Compiler.Assembler.X86;
        // using Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86
        // {
        //     [OpCode(OpCodeEnum.Conv_I1)]
        //     public class Conv_I1 : Op
        //     {
        //         private string mNextLabel;
        //         private string mCurLabel;
        //         private uint mCurOffset;
        //         private MethodInformation mMethodInformation;
        //         public Conv_I1(ILReader aReader, MethodInformation aMethodInfo)
        //             : base(aReader, aMethodInfo)
        //         {
        //             mMethodInformation = aMethodInfo;
        //             mCurOffset = aReader.Position;
        //             mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        //         }
        //         public override void DoAssemble()
        //         {
        //             StackContent xSource = Assembler.Stack.Pop();
        //             if (xSource.IsFloat)
        //             {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: Floats not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                 return;
        //             }
        //             switch (xSource.Size)
        //             {
        //                 case 1:
        //                     new CPUx86.Noop();
        //                     break;
        //                 case 2:
        //                 case 4:
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                     new CPUx86.SignExtendAX { Size = 8 };
        //                     new CPUx86.SignExtendAX { Size = 16 };
        //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        //                     break;
        //                 case 8:
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
        //                     new CPUx86.SignExtendAX { Size = 8 };
        //                     new CPUx86.SignExtendAX { Size = 16 };
        //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        //                     break;
        //                 default:
        //                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                     return;
        //             }
        //             Assembler.Stack.Push(new StackContent(1, true, false, true));
        //         }
        //     }
        // }

    }
}