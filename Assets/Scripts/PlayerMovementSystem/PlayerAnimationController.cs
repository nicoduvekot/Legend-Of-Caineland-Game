using UnityEngine;

namespace PlayerMovementSystem
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public void SetMove(bool isMoving)
        {
            animator.SetBool("1_Move", isMoving);
        }
        
        public void PlayAttack()
        {
            animator.SetTrigger("2_Attack");
        }

        public void PlayDamaged()
        {
            animator.SetTrigger("3_Damaged");
        }

        public void PlayDeath()
        {
            animator.SetTrigger("4_Death");
        }
    }
}