#if NETSTANDARD2_0

// Compiler shims required to use C# 9+ features (records, init, required)
// when targeting netstandard2.0, where these runtime types are absent.

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>Enables <c>init</c>-only property setters on netstandard2.0.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }

    /// <summary>Enables the <c>required</c> modifier on netstandard2.0.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    /// <summary>Signals that a feature is required by the compiler on netstandard2.0.</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;
        public string FeatureName { get; }
        public bool IsOptional { get; set; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Marks a constructor as setting all required members on netstandard2.0.</summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}

#endif
