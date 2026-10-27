using PANiXiDA.TacticalHeroes.Players.Domain.Files;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Domain.Players;

public sealed class PlayerTests
{
    [Fact(DisplayName = "Player should start with a linked user and initial rating when created")]
    public void Create_Should_SetProfileAndInitialRating_When_Created()
    {
        var userId = UserId.New();

        var player = Player.Create(userId);

        player.Id.Value.ShouldNotBe(Guid.Empty);
        player.Id.Value.Version.ShouldBe(7);
        player.UserId.ShouldBe(userId);
        player.AvatarFileId.ShouldBeNull();
        player.Rating.Value.ShouldBe(1000);
    }

    [Fact(DisplayName = "Player should replace its avatar when another file is selected")]
    public void ChangeAvatar_Should_ReplaceAvatar_When_AnotherFileIsSelected()
    {
        var player = CreatePlayer();
        player.ChangeAvatar(FileId.New());
        var avatarFileId = FileId.New();

        player.ChangeAvatar(avatarFileId);

        player.AvatarFileId.ShouldBe(avatarFileId);
        player.Rating.Value.ShouldBe(1000);
    }

    [Fact(DisplayName = "Player should clear its avatar when removal is repeated")]
    public void RemoveAvatar_Should_ClearAvatar_When_RemovalIsRepeated()
    {
        var player = CreatePlayer();
        player.ChangeAvatar(FileId.New());

        player.RemoveAvatar();
        player.RemoveAvatar();

        player.AvatarFileId.ShouldBeNull();
        player.Rating.Value.ShouldBe(1000);
    }

    [Theory(DisplayName = "Player should preserve its profile when rating is changed")]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(1500)]
    public void ChangeRating_Should_PreserveProfile_When_RatingIsChanged(int value)
    {
        var player = CreatePlayer();
        var userId = player.UserId;
        var avatarFileId = FileId.New();
        player.ChangeAvatar(avatarFileId);

        player.ChangeRating(Rating.Create(value));

        player.UserId.ShouldBe(userId);
        player.AvatarFileId.ShouldBe(avatarFileId);
        player.Rating.Value.ShouldBe(value);
    }

    private static Player CreatePlayer()
    {
        return Player.Create(UserId.New());
    }
}
