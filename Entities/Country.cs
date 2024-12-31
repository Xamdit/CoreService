namespace Service.Entities;

public partial class Country
{
    public int Id { get; set; }

    public string Iso2 { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public string LongName { get; set; } = null!;

    public string Iso3 { get; set; } = null!;

    public string Numcode { get; set; } = null!;

    public string UnMember { get; set; } = null!;

    public string CallingCode { get; set; } = null!;

    public string Cctld { get; set; } = null!;

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
}
