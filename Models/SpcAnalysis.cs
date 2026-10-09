namespace QualiTrack.Models;

public class SpcAnalysis
{
    public Guid Id { get; set; }
    public string ParameterName { get; set; } = string.Empty;  // Nama produk/parameter
    public string? Description { get; set; }
    public double Lsl { get; set; }
    public double Usl { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double Ucl { get; set; }
    public double Lcl { get; set; }
    public double Cp { get; set; }
    public double Cpk { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsStable { get; set; }  
    public int DataCount { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;  
    public Guid AnalyzedById { get; set; }
    public User? AnalyzedBy { get; set; }
    public double? Target { get; set; }
    public string? Unit { get; set; }
    public List<double> DataPoints { get; set; } = [];
 }