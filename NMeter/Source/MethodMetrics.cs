namespace NMeter
{
    public class MethodMetrics
    {
        public string Signature;
        public int ParameterCount;
        public int InstructionCount;
        public bool IsGenerated;
        public bool IsStatic;
        public byte[] Fingerprint;
    }
}