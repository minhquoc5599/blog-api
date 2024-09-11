namespace Blog.Core.Configs
{
    public class JwtTokenSettings
    {
        public string Key { get; set; }
        public string Isuser { get; set; }
        public int ExpireHours { get; set; }
    }
}
