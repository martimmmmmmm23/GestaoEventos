namespace GestãoEventos.Data.Classes
{
    public class Participante
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}
