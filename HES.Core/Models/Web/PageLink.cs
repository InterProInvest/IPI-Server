namespace HES.Core.Models.Web
{
    public class PageLink
    {
        public PageLink(int page) : this(page, true) { }
        public PageLink(int page, bool enabled) : this(page, enabled, page.ToString()) { }
        public PageLink(int page, bool enabled, string text)
        {
            Page = page;
            Enabled = enabled;
            Text = text;
        }

        public string Text { get; set; }
        public int Page { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Active { get; set; } = false;
    }
}
