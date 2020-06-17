using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    /// <summary>
    /// ILGenerator 扩展类
    /// </summary>
    public static class ILGeneratorExtensions
    {
        /// <summary>
        /// 将参数（由指定索引值引用）加载到堆栈上
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="arg"></param>
        public static void Ldarg(this ILGenerator generator, int arg)
        {
            switch (arg)
            {
                case 0:
                    generator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (arg <= byte.MaxValue)
                    {
                        generator.Emit(OpCodes.Ldarg_S, (byte)arg);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Ldarg, (ushort)arg);
                    }
                    break;
            }
        }

        /// <summary>
        /// 将所提供的 int32 类型的值作为 int32 推送到计算堆栈上
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="arg"></param>
        public static void Ldc_I4(this ILGenerator generator, int arg)
        {
            switch (arg)
            {
                case -1:
                    generator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    generator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    generator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    generator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    generator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    generator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    generator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (arg >= -128 && arg <= 127)
                    {
                        generator.Emit(OpCodes.Ldc_I4_S, arg);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Ldc_I4, arg);
                    }
                    break;
            }
        }

        /// <summary>
        /// 将所提供的 int64 类型的值作为 int64 推送到计算堆栈上
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="arg"></param>
        public static void Ldc_I8(this ILGenerator generator, long arg)
        {
            generator.Emit(OpCodes.Ldc_I8, arg);
        }

        /// <summary>
        /// 将所提供的 float32 类型的值作为 F (float) 类型推送到计算堆栈上
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="arg"></param>
        public static void Ldc_R4(this ILGenerator generator, float arg)
        {
            generator.Emit(OpCodes.Ldc_R4, arg);
        }

        /// <summary>
        /// 将所提供的 float64 类型的值作为 F (float) 类型推送到计算堆栈上
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="arg"></param>
        public static void Ldc_R8(this ILGenerator generator, double arg)
        {
            generator.Emit(OpCodes.Ldc_R8, arg);
        }
    }
}