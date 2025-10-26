using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider))]
public class MagnetGrabber : MonoBehaviour
{
    [Header("Magnet detection")]
    [Tooltip("Radio para buscar objetos agarrables")]
    public float radius = 1.2f;

    [Tooltip("Fuerza aplicada para atraer hacia el HoldPoint")]
    public float attractionStrength = 25f;

    [Tooltip("Velocidad deseada máxima al atraer")]
    public float maxAttractionSpeed = 6f;

    [Tooltip("Si la distancia es menor que esto, hace snap inmediato")]
    public float snapDistance = 0.15f;

    [Tooltip("Capas de objetos agarrables (ej: Grabbable)")]
    public LayerMask layerMask;

    [Header("Hold point & attach")]
    [Tooltip("Punto donde se coloca el objeto agarrado")]
    public Transform holdPoint;

    [Tooltip("Usar FixedJoint (física realista) o parenting (arcade)")]
    public bool useFixedJoint = true;

    public float jointBreakForce = 2000f;
    public float jointBreakTorque = 2000f;

    [Header("Behavior")]
    [Tooltip("Si está en true, el imán está activo siempre")]
    public bool magnetAlwaysOn = false;

    [Tooltip("Al soltar, hereda velocidad del end-effector")]
    public bool inheritVelocityOnRelease = true;

    [Tooltip("Impulso extra hacia adelante al soltar")]
    public float throwBoost = 0f;

    [Tooltip("Tecla para alternar el imán si magnetAlwaysOn = false")]
    public KeyCode toggleKey = KeyCode.E;

    // Estado interno
    private Rigidbody grabbedRb;
    private FixedJoint fixedJoint;
    private bool magnetEnabled;


    void Awake()
    {
        // Asegurar SphereCollider como trigger
        var sc = GetComponent<SphereCollider>();
        if (sc == null) sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = radius;

        // Asegurar Rigidbody cinemático en el imán (muy importante para FixedJoint)
        var effRb = GetComponent<Rigidbody>();
        if (effRb == null) effRb = gameObject.AddComponent<Rigidbody>();
        effRb.isKinematic = true;
        effRb.useGravity = false;

        if (holdPoint == null) holdPoint = transform;
        magnetEnabled = magnetAlwaysOn;

        Debug.Log($"[MagnetGrabber] Awake en '{name}'. " +
                  $"radius={radius}, snapDistance={snapDistance}, useFixedJoint={useFixedJoint}, " +
                  $"magnetAlwaysOn={magnetAlwaysOn}, layerMask={layerMask}");
    }

    void OnValidate()
    {
        // Mantener el radio del trigger en edición
        var sc = GetComponent<SphereCollider>();
        if (sc != null)
        {
            sc.isTrigger = true;
            sc.radius = radius;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
        if (holdPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(holdPoint.position, snapDistance);
        }
    }

    void Update()
    {
        if (!magnetAlwaysOn && Input.GetKeyDown(toggleKey))
            ToggleMagnet();
    }

    void FixedUpdate()
    {
        if (!magnetEnabled || grabbedRb != null) return;

        // Busca cuerpos en el radio
        var hits = Physics.OverlapSphere(transform.position, radius, layerMask, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            Debug.Log($"[MagnetGrabber] {hits.Length} collider(s) en radio.");
        }

        Rigidbody target = null;
        float bestDist = Mathf.Infinity;

        foreach (var h in hits)
        {
            if (!h.attachedRigidbody) continue;
            var rb = h.attachedRigidbody;
            if (rb.isKinematic) continue;

            float d = Vector3.Distance(rb.worldCenterOfMass, holdPoint.position);
            if (d < bestDist)
            {
                bestDist = d;
                target = rb;
            }
        }

        if (target != null)
        {
            Debug.Log($"[MagnetGrabber] Candidato: '{target.name}', dist={bestDist:0.###}");

            Vector3 toPoint = (holdPoint.position - target.worldCenterOfMass);
            float dist = toPoint.magnitude;

            if (dist <= snapDistance)
            {
                Debug.Log($"[MagnetGrabber] A punto de SNAP con '{target.name}' (dist={dist:0.###}).");
                SnapToHold(target);
            }
            else
            {
                // Atraer con límite de velocidad
                Vector3 desiredVel = toPoint.normalized * maxAttractionSpeed;
                Vector3 velChange = desiredVel - target.linearVelocity;
                Vector3 force = Vector3.ClampMagnitude(velChange * target.mass, attractionStrength);
                target.AddForce(force, ForceMode.Force);
                Debug.Log($"[MagnetGrabber] Atrayendo '{target.name}' con F={force.magnitude:0.##} (dist={dist:0.###}).");
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Snap redundante si entra muy cerca
        if (!magnetEnabled || grabbedRb != null) return;
        if (((1 << other.gameObject.layer) & layerMask) == 0) return;
        if (!other.attachedRigidbody || other.attachedRigidbody.isKinematic) return;

        float d = Vector3.Distance(other.attachedRigidbody.worldCenterOfMass, holdPoint.position);
        if (d <= snapDistance)
        {
            Debug.Log($"[MagnetGrabber] OnTriggerStay SNAP con '{other.attachedRigidbody.name}' (dist={d:0.###}).");
            SnapToHold(other.attachedRigidbody);
        }
    }

    public void ToggleMagnet()
    {
        magnetEnabled = !magnetEnabled;
        Debug.Log($"[MagnetGrabber] Magnet {(magnetEnabled ? "ON" : "OFF")} en '{name}'.");
        if (!magnetEnabled && grabbedRb) Release();
    }

    public void GrabNowNearest()
    {
        magnetEnabled = true;
        Debug.Log("[MagnetGrabber] GrabNowNearest: magnet ON.");
    }

    void SnapToHold(Rigidbody rb)
    {
        // Colocar y alinear inmediatamente
        rb.MovePosition(holdPoint.position);
        rb.MoveRotation(holdPoint.rotation);

        if (useFixedJoint)
        {
            fixedJoint = gameObject.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = rb;
            fixedJoint.breakForce = jointBreakForce;
            fixedJoint.breakTorque = jointBreakTorque;
            fixedJoint.enablePreprocessing = false;

            Debug.Log($"[MagnetGrabber] SNAP con FixedJoint a '{rb.name}'. breakF={jointBreakForce}, breakT={jointBreakTorque}");
        }
        else
        {
            rb.isKinematic = true;
            rb.transform.SetPositionAndRotation(holdPoint.position, holdPoint.rotation);
            rb.transform.SetParent(holdPoint, true);

            Debug.Log($"[MagnetGrabber] SNAP por parenting a '{rb.name}'. isKinematic=true");
        }

        grabbedRb = rb;
    }

    public void Release()
    {
        if (!grabbedRb) return;

        if (useFixedJoint)
        {
            if (fixedJoint)
            {
                Destroy(fixedJoint);
                Debug.Log($"[MagnetGrabber] FixedJoint destruido. Release de '{grabbedRb.name}'.");
            }
        }
        else
        {
            grabbedRb.transform.SetParent(null, true);
            grabbedRb.isKinematic = false;
            Debug.Log($"[MagnetGrabber] Parenting deshecho. Release de '{grabbedRb.name}'.");
        }

        if (inheritVelocityOnRelease)
        {
            Vector3 effVel = Vector3.zero;
            var rbEffector = GetComponentInParent<Rigidbody>();
            if (rbEffector) effVel = rbEffector.linearVelocity;

            grabbedRb.linearVelocity = effVel + transform.forward * throwBoost;
            Debug.Log($"[MagnetGrabber] Velocidad aplicada al soltar: {grabbedRb.linearVelocity} (boost={throwBoost}).");
        }

        grabbedRb = null;
        fixedJoint = null;
    }

    void OnJointBreak(float breakForce)
    {
        Debug.Log($"[MagnetGrabber] ¡Joint roto! Fuerza={breakForce:0.##}. Limpiando estado.");
        grabbedRb = null;
        fixedJoint = null;
    }
}

