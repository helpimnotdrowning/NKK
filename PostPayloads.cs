namespace NKK;

public class PostPayload {
	public String Title { get; set; };
	public String Description { get; set; };
	public DateTime Date { get; set; };

	public PostPayload(String title, String description, DateTime date) {
		this.Title = title;
		this.Description = description;
		this.Date = date;
	}
	
	public PostPayload(String title, String description, String date) {
		this.Title = title;
		this.Description = description;
		this.Date = DateTime.Parse($"{date}Z").ToUniversalTime();
	}
}