namespace GestãoEventos.Data.Classes
{
    public class Inscricao
    {
        public int EventoId { get; set; } // Chave estrangeira para o evento
        public Evento? Evento { get; set; } // Propriedade de navegação para o evento

        public int ParticipanteId { get; set; }
        public Participante? Participante { get; set; }
    }
}
