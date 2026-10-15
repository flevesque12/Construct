using UnityEngine;

namespace Construct.Core
{
    public interface IState
    {
        //est appelé une seule fois, quand on entre dans l’état. C’est là qu’on lance une animation, active une hitbox ou remet un timer à zéro.
        void Enter();
        //est appelé à chaque frame tant qu’on est dans l’état. C’est la logique continue : se déplacer, vérifier les inputs, décider d’une transition.
        void Tick(float deltaTime);
        //est appelé une seule fois, quand on quitte l’état. On y nettoie : désactiver la hitbox, remettre la vitesse normale, etc.
        void Exit();
    }
}
