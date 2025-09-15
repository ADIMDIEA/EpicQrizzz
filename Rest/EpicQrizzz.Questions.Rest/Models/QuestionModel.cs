namespace EpicQrizzz.Questions.Rest.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<Awnser> Options { get; set; }

    }

    public class FullAwnser
    {
        public char CorrectOption { get; set; }

    }

}
