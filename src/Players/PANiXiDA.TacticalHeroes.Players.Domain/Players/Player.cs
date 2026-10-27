using PANiXiDA.TacticalHeroes.Players.Domain.Files;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.Domain.Players;

public sealed class Player : AggregateRoot<PlayerId>
{
    private Player(
        PlayerId id,
        UserId userId,
        Rating rating)
        : base(id)
    {
        UserId = userId;
        Rating = rating;
    }

    public UserId UserId { get; private set; }
    public FileId? AvatarFileId { get; private set; }
    public Rating Rating { get; private set; }

    public static Player Create(UserId userId)
    {
        return new Player(
            id: PlayerId.New(),
            userId: userId,
            rating: Rating.Create(Rating.InitialValue));
    }

    public void ChangeAvatar(FileId avatarFileId)
    {
        AvatarFileId = avatarFileId;
    }

    public void RemoveAvatar()
    {
        AvatarFileId = null;
    }

    public void ChangeRating(Rating rating)
    {
        Rating = rating;
    }
}
