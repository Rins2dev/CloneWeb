namespace ViewModel
{
    public class Response
    {
        public int code { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public int recordsFiltered { get; set; }
    }
}
