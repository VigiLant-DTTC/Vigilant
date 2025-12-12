using System.ComponentModel.DataAnnotations;
using VigiLant.Models.Enum;

namespace VigiLant.Models
{
    public class AppConfig
    {
        public int Id { get; set; } = 1; 
        public string MqttHost { get; set; } = "broker.emqx.io"; 
        public string MqttTopicWildcard { get; set; } = "vigilant/data/#"; 
        public MqttPorta MqttPort { get; set; }  = MqttPorta.Porta1883;
    }
}