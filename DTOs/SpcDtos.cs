namespace QualiTrack.DTOs;

public class SpcResultDto
{
    public Guid Id { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double Ucl { get; set; }
    public double Lcl { get; set; }
    public double Lsl { get; set; }
    public double Usl { get; set; }
    public double Cp { get; set; }
    public double Cpk { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsStable { get; set; }
    public List<double> OutOfControlPoints { get; set; } = [];
    public int DataCount { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public List<double> Data { get; set; } = [];
    public double? Target { get; set; }
    public string? Unit { get; set; }
        
}

public class SpcHistoryDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Cp { get; set; }
    public double Cpk { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsStable { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public double? Target { get; set; }
    public string? Unit { get; set; }
}