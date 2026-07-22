namespace NutriTrack.Domain.Enums;

/// <summary>
/// Controls who can see a user-created recipe or ingredient.
///   <see cref="Private"/>  — Only the author (and admins) can see it.
///   <see cref="Unlisted"/> — Author has opted in for potential publication;
///                            still only visible to author + admins.
///   <see cref="Public"/>   — Admin approved; visible to everyone.
///   <see cref="Rejected"/> — Admin reviewed and declined; visible only to
///                            author + admins. Author can re-submit.
/// </summary>
public enum EntryVisibility
{
    Private,
    Unlisted,
    Public,
    Rejected,
}