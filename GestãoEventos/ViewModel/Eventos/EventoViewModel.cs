using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestãoEventos.Data.Classes;
using Newtonsoft.Json.Serialization;

namespace GestãoEventos.ViewModel.Eventos
{
    public class EventoViewModel
    {
        [Required(ErrorMessage = "O campo Nome é de preenchimento obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O campo Data é obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "O campo Local é de preenchimento obrigatório.")]
        public string Local { get; set; }

        [Display(Name = "Imagem")]
        public string? Image { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Descrição")]
        [StringLength(500)]
        public string? Descricao { get; set; }

        [Display(Name = "Hora")]
        [Required(ErrorMessage = "O campo Hora é obrigatório.")]
        public TimeSpan Hora { get; set; }

        [Display(Name = "Preço")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Valor impossível! O preço não pode exceder 99.999.999,99.")]
        public decimal? Preco { get; set; }

        [Display(Name = "Total de Lugares")]
        [Range(1, 80000, ErrorMessage = "O número de lugares não pode exceder os 80.000.")]
        public int? Lugares { get; set; } // null indica que não há limite de lugares

        public ICollection<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}