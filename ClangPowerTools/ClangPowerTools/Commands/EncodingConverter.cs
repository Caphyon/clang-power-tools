using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Commands
{
    public class EncodingConverter: BasicCommand
    {
        public EncodingConverter(CommandController aCommandController, OleMenuCommandService aCommandService, AsyncPackage aPackage, Guid aGuid, int aId)
            :base(aPackage, aGuid, aId)
        {

        }
    }
}
