using nexRemoteFree.Shared.Attributes;
using nexRemoteFree.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace nexRemoteFree.Shared.Models
{
    public class Device
    {
        [Sortable]
        [Display(Name = "Wersja Agenta")]
        public string AgentVersion { get; set; }

        public ICollection<Alert> Alerts { get; set; }
        [StringLength(100)]

        [Sortable]
        [Display(Name = "Alias")]
        public string Alias { get; set; }

        [Sortable]
        [Display(Name = "Utylizacja CPU")]
        public double CpuUtilization { get; set; }

        [Sortable]
        [Display(Name = "Bieżacy użytkownik")]
        public string CurrentUser { get; set; }

        public DeviceGroup DeviceGroup { get; set; }
        public string DeviceGroupID { get; set; }

        [Sortable]
        [Display(Name = "Nazwa urządzenia")]
        public string DeviceName { get; set; }
        public List<Drive> Drives { get; set; }

        [Key]
        public string ID { get; set; }

        public bool Is64Bit { get; set; }
        public bool IsOnline { get; set; }

        [Sortable]
        [Display(Name = "Ostatnio Online")]
        public DateTimeOffset LastOnline { get; set; }

        [StringLength(5000)]
        public string Notes { get; set; }       

        [JsonIgnore]
        public Organization Organization { get; set; }

        public string OrganizationID { get; set; }
        public Architecture OSArchitecture { get; set; }

        [Sortable]
        [Display(Name = "Opis Systemu")]
        public string OSDescription { get; set; }

        [Sortable]
        [Display(Name = "Platforma")]
        public string Platform { get; set; }

        [Sortable]
        [Display(Name = "Liczba CPU")]
        public int ProcessorCount { get; set; }

        public string PublicIP { get; set; }
        public string ServerVerificationToken { get; set; }

        [JsonIgnore]
        public List<ScriptResult> ScriptResults { get; set; }

        [JsonIgnore]
        public List<ScriptRun> ScriptRuns { get; set; }
        [JsonIgnore]
        public List<ScriptRun> ScriptRunsCompleted { get; set; }

        [JsonIgnore]
        public List<ScriptSchedule> ScriptSchedules { get; set; }

        [StringLength(200)]
        [Sortable]
        [Display(Name = "Tagi")]
        public string Tags { get; set; } = "";

        [Sortable]
        [Display(Name = "RAM Total")]
        public double TotalMemory { get; set; }

        [Sortable]
        [Display(Name = "Całkowita Przestrzeń")]
        public double TotalStorage { get; set; }

        [Sortable]
        [Display(Name = "RAM w użyciu")]
        public double UsedMemory { get; set; }

        [Sortable]
        [Display(Name = "RAM w użyciu %")]
        public double UsedMemoryPercent => UsedMemory / TotalMemory;

        [Sortable]
        [Display(Name = "Przestrzeń w użyciu")]
        public double UsedStorage { get; set; }

        [Sortable]
        [Display(Name = "Przestrzeń w użyciu %")]
        public double UsedStoragePercent => UsedStorage / TotalStorage;

        public WebRtcSetting WebRtcSetting { get; set; }
    }
}