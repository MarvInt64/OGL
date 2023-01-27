#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vN;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform vec3 uCameraPosition;

out vec3 Normal;
out vec3 FragPos;
out vec3 CameraPos;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    Normal = vN;
    FragPos = vPos;
    CameraPos = uCameraPosition;
}
