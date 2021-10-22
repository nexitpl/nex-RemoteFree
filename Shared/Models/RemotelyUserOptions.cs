using nexRemote.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace nexRemote.Shared.Models
{
    public class nexRemoteUserOptions
    {
        [Display(Name = "Nazwa wyświetlana")]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [Display(Name = "PS Core skrót")]
        [StringLength(10)]
        public string CommandModeShortcutPSCore { get; set; } = "/pscore";
        [Display(Name = "Powershell skrót")]
        [StringLength(10)]
        public string CommandModeShortcutWinPS { get; set; } = "/winps";
        [Display(Name = "CMD skrót")]
        [StringLength(10)]
        public string CommandModeShortcutCMD { get; set; } = "/cmd";
        [Display(Name = "Bash skrót")]
        [StringLength(10)]
        public string CommandModeShortcutBash { get; set; } = "/bash";

        [Display(Name = "Motyw")]
        public Theme Theme { get; set; }
    }
}
