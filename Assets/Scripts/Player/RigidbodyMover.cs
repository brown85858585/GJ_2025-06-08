using UnityEngine;

namespace Player
{
    public class RigidbodyMover
    {
        // внутренний индекс текущей цели
        private static int currentIndex = 0;
        
        
        public static void ResetIndex()
        {
            currentIndex = 0;
        }
        /// <summary>
        /// Перемещает Rigidbody последовательно по массиву точек.
        /// </summary>
        /// <param name="rb">Физическое тело, которое будем двигать.</param>
        /// <param name="points">Массив целевых позиций.</param>
        /// <param name="moveSpeed">Желаемая линейная скорость движения.</param>
        /// <param name="rotationSpeed">Скорость поворота (градусы в секунду).</param>
        /// <param name="stopDistance">Расстояние до цели, при котором считается, что точка достигнута.</param>
        /// <param name="isLoop">перемещаться по точкам, по кругу</param>
        public static void MoveAlongPointsUpdate(
            Rigidbody rb,
            Vector3[] points,
            float moveSpeed,
            float rotationSpeed = 6,
            float stopDistance = 0.5f,
            bool isLoop = false
        )
        {
            
            // если все точки пройдены — выходим
            if (currentIndex > points.Length - 1)
            {
                if(isLoop) currentIndex = 0; // сбрасываем индекс для следующего цикла
                return;
            }

            Vector3 target = points[currentIndex];

            // 1. Движение к точке
            Vector3 toTarget = (target - rb.position).normalized;
            Vector3 desiredV = toTarget * moveSpeed;
            Vector3 deltaV = desiredV - rb.velocity;
            // Заменили Time.deltaTime на Time.fixedDeltaTime, т.к. AddForce лучше в Fixed
            Vector3 force = rb.mass * deltaV / Time.fixedDeltaTime;
            rb.AddForce(force, ForceMode.Force);

            // 2. Сглаженный поворот
            Quaternion targetRot = Quaternion.LookRotation(toTarget);
            float t = Mathf.Min(1f, rotationSpeed * Time.fixedDeltaTime);
            Quaternion smooth = Quaternion.Slerp(rb.rotation, targetRot, t);
            rb.MoveRotation(smooth);

            // 3. Проверка достижения текущей точки
            if ((rb.position - target).sqrMagnitude <= stopDistance * stopDistance)
            {
                currentIndex++;
            }
        }
    }
}