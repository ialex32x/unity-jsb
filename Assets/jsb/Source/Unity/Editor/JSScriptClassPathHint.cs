namespace QuickJS.Unity
{
    public struct JSScriptClassPathHint
    {
        public readonly string sourceFile;
        public readonly string modulePath;
        public readonly string className;
        public readonly string classPath;

        public JSScriptClassPathHint(string sourceFile, string modulePath, string className)
        {
            this.sourceFile = sourceFile;
            this.modulePath = modulePath;
            this.className = className;
            this.classPath = JSBehaviourScriptRef.ToClassPath(modulePath, className);
        }

        public bool IsReferenced(JSBehaviourScriptRef scriptRef)
        {
            return scriptRef.sourceFile == sourceFile && scriptRef.modulePath == modulePath && scriptRef.className == className;
        }

        public string ToClassPath()
        {
            return classPath;
        }
    }
}