namespace FmScout.Application.Players;
public sealed record PlayerDto(Guid Id,string Name,string Slug,int Age,string Position,string Club,string Nation,int CurrentAbility,int PotentialAbility,decimal MarketValue,bool IsWonderkid,bool IsHiddenGem);
public sealed record CreatePlayerRequest(string Name,int Age,string Position,string Club,string Nation,int CurrentAbility,int PotentialAbility,decimal MarketValue,bool IsWonderkid,bool IsHiddenGem);
