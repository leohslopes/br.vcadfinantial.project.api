namespace br.vcadfinantial.project.domain.Agreggate
{
    public class ReportLogInfoAgreggate
    {
        public required string MounthKey { get; set; }

        public required string FileName { get; set; }

        public required string OfficialNumber { get; set; }

        public bool Active { get; set; }

        public long AccountKey { get; set; }

        public decimal Among { get; set; }

        
    }
}

