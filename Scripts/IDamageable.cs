namespace Valor;

/// <summary>
/// interface to signify this class is able to take damage
/// </summary>
public interface IDamageable
{
	public float Health { get; }

	public void Hurt(float damage);
}