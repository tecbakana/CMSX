//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace mvcAlivre.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Envio_Vaga
    {
        public int NumEnvio { get; set; }
        public Nullable<int> NumVaga { get; set; }
        public string NomeDe { get; set; }
        public string EmailDe { get; set; }
        public string NomePara { get; set; }
        public string EmailPara { get; set; }
        public string Comentario { get; set; }
        public Nullable<int> StatusLeit { get; set; }
        public Nullable<System.DateTime> DtEnvio { get; set; }
        public Nullable<System.DateTime> DtLeit { get; set; }
    }
}
