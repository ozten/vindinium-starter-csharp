namespace Vindinium.Interfaces
{
	public interface IApiEndpoints
	{
		IApiRequest StartArena();
		IApiRequest StartTraining(uint turns);
		IApiRequest Play(string playUrl, Direction direction);
	}
}