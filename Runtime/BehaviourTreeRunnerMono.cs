using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class BehaviourTreeRunnerMono : MonoBehaviour
    {
        [SerializeField] public BehaviourTree BehaviourTree;
        private BehaviourTreeRunner _runner;
        public BehaviourTreeRunner Runner => _runner;

        private void Awake()
        {
            _runner = new(BehaviourTree);
        }

        private void OnEnable()
        {
            _runner.Prewarm(gameObject);
        }

        private void OnDisable()
        {
            _runner.Dispose();
        }

        private void Update()
        {
            _runner.UpdateTree(Time.deltaTime);
        }
    }
}