using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_issue_937
{
    public abstract class TemplateBase
    {
        StringBuilder stringBuilder = new StringBuilder();
        public abstract Task ExecuteAsync();

        public virtual void Write(object value)
        {
            stringBuilder.Append(value);
            //stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("\r\n");
        }

        public virtual void WriteLiteral(object value)
        {
            stringBuilder.Append(value);
        }
    }
}
