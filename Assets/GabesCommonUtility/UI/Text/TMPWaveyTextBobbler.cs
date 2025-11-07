using System.Collections;
using TMPro;
using UnityEngine;


namespace GabesCommonUtility.UI.Text
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPWaveyTextBobbler : MonoBehaviour
    {
        [Tooltip("Amplitude of the wave, how high the characters move.")]
        public float amplitude = 5f;
    
        [Tooltip("Frequency of the wave, how often the characters move.")]
        public float frequency = 1f;
        
        public float initialDelay = 1f;

        private TMP_Text textMesh;
        private TMP_TextInfo textInfo;
        private Vector3[][] originalVertices; // Cache original positions
        private int characterCount = -1;
        private bool meshInfoCached = false;
        
        // Pre-allocate arrays to avoid GC
        private Vector2[] offsetCache;
        private float timeOffset;

        private void OnEnable()
        {
            if (!textMesh)
            {
                textMesh = GetComponent<TMP_Text>();
            }
            
            StartCoroutine(InitializeAndAnimate());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator InitializeAndAnimate()
        {
            yield return new WaitForSeconds(initialDelay);
            
            // Force initial mesh update and cache everything
            textMesh.ForceMeshUpdate();
            CacheMeshInfo();
            
            while (true)
            {
                // Only update if character count changed (text changed)
                if (textInfo.characterCount != characterCount)
                {
                    CacheMeshInfo();
                }
                
                AnimateVertices();
                UpdateMeshes();
                
                yield return null;
            }
        }
        
        private void CacheMeshInfo()
        {
            textInfo = textMesh.textInfo;
            characterCount = textInfo.characterCount;
            
            // Cache original vertex positions
            if (originalVertices == null || originalVertices.Length != textInfo.meshInfo.Length)
            {
                originalVertices = new Vector3[textInfo.meshInfo.Length][];
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    originalVertices[i] = new Vector3[textInfo.meshInfo[i].vertices.Length];
                }
            }
            
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                System.Array.Copy(textInfo.meshInfo[i].vertices, originalVertices[i], textInfo.meshInfo[i].vertices.Length);
            }
            
            // Pre-allocate offset cache
            if (offsetCache == null || offsetCache.Length < characterCount)
            {
                offsetCache = new Vector2[Mathf.Max(characterCount, 32)]; // Min size to avoid frequent reallocations
            }
            
            meshInfoCached = true;
        }
        
        private void AnimateVertices()
        {
            if (!meshInfoCached || characterCount == 0) return;
            
            float currentTime = Time.time;
            
            // Pre-calculate all offsets (more cache friendly)
            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                {
                    offsetCache[i] = Vector2.zero;
                    continue;
                }
                
                // Inline the wobble calculation to avoid function call overhead
                float wobbleTime = currentTime + i;
                offsetCache[i] = new Vector2(0, Mathf.Sin(wobbleTime * frequency) * amplitude);
            }
            
            // Apply offsets to vertices
            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                var charInfo = textInfo.characterInfo[i];
                int vertexIndex = charInfo.vertexIndex;
                int materialIndex = charInfo.materialReferenceIndex;
                
                var vertices = textInfo.meshInfo[materialIndex].vertices;
                var originalVerts = originalVertices[materialIndex];
                Vector3 offset = offsetCache[i];

                // Apply offset to all 4 vertices of the character
                vertices[vertexIndex] = originalVerts[vertexIndex] + offset;
                vertices[vertexIndex + 1] = originalVerts[vertexIndex + 1] + offset;
                vertices[vertexIndex + 2] = originalVerts[vertexIndex + 2] + offset;
                vertices[vertexIndex + 3] = originalVerts[vertexIndex + 3] + offset;
            }
        }
        
        private void UpdateMeshes()
        {
            // Only update meshes that actually have characters
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                if (meshInfo.vertices.Length > 0)
                {
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    textMesh.UpdateGeometry(meshInfo.mesh, i);
                }
            }
        }
    }
}