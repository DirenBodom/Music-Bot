namespace BronnBot
{
    public class Song {
        public string artist { get; private set; }
        public string name { get; private set; }
        public string album { get; private set; }
        public string url { get; private set; }
        public Song(string a, string n, string al, string u) {
            this.artist = a;
            name = n;
            album = al;
            url = u;
        }
    }
}