namespace ArticleApi.Models;

using System.ComponentModel.DataAnnotations; 

public enum Badge 
{
    Poster,

    [Display(Name = "Waste Warrior")]
    WasteWarrior,

    [Display(Name = "Road Guardian")]
    RoadGuardian,

    [Display(Name = "Community Helper")]
    CommunityHelper,

    Socialite,
}