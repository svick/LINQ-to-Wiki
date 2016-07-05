using System.Xml.Linq;
// <copyright file="XProcessingInstructionFactory.cs">Copyright ©  2011</copyright>

using System;
using Microsoft.Pex.Framework;

namespace System.Xml.Linq
{
    /// <summary>A factory for System.Xml.Linq.XProcessingInstruction instances</summary>
    public static partial class XProcessingInstructionFactory
    {
        /// <summary>A factory for System.Xml.Linq.XProcessingInstruction instances</summary>
        [PexFactoryMethod(typeof(XProcessingInstruction))]
        public static XProcessingInstruction Create(string target, string data)
        {
            XProcessingInstruction xProcessingInstruction = new XProcessingInstruction(target, data);
            return xProcessingInstruction;
        }
    }
}
