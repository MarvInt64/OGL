#version 330 core
in vec3 Normal;
in vec3 FragPos;
in vec3 CameraPos;
out vec4 FragColor;

uniform float specular_coefficient = 0.1f;
uniform vec3 light_position;
uniform vec3 specular_color;
uniform float shininess = 0.1f;
uniform float deltaTime;
uniform sampler2D Texture0;
uniform sampler2D Texture1;

void main()
{
    
    /*
    vec3 c = vec3(CameraPos);
    vec3 lightPos =  CameraPos; //vec3(200.0f, 200.0f, 50.0f);
    vec3 lightColor = vec3(0.7f, 0.7f, 0.7f);
    vec3 lightDir = normalize(lightPos - FragPos);
    
    // Calculate the diffuse lighting
    float diff = clamp(dot(Normal, lightDir), 0, 1);
    //vec3 diffuse = diff * lightColor;

    vec3 diffuse = max(dot(lightDir, Normal), 0.0) * lightColor;
    
    FragColor = vec4(vec3(0.4f, 0.7f, 0.0f) * diffuse, 1.0f);
    // FragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f) * vec4(Normal, 1.0);
    */
    
    /*
    vec3 lightPos = vec3(200.0f, 200.0f, 150.0f);
    vec3 view_vector = normalize(CameraPos - FragPos);
    vec3 light_color = vec3(0.992f, 0.984f, 0.827f);
    vec3 light_vector = normalize(lightPos - FragPos);
    vec3 ambient_light_color = vec3(0.5f, 0.5f, 0.5f);
    float ambient_coefficient = 0.2f;
    vec3 specular_color = vec3(0.6f, 0.6f, 0.6f);
    float specular_coefficient = 0.01f;
    float shininess = 0.1f;
    
    vec3 diffuse = max(dot(light_vector, Normal), 0.0) * light_color;
    vec3 ambient = ambient_light_color * ambient_coefficient;
    vec3 specular = pow(max(dot(reflect(-light_vector, Normal), view_vector), 0.0), shininess) * specular_color * specular_coefficient;
    vec3 lighting = diffuse + ambient + specular;
    FragColor = vec4(lighting, 1.0f);
    */
    
   
     vec3 view_position = normalize(CameraPos - FragPos);
     
     vec3 light_color = vec3(0.992f, 0.984f, 0.827f);
     vec3 diffuse_color = vec3(0.94f, 0.94f, 0.94f);
     vec3 ambient_color = vec3(0.5f, 0.5f, 0.5f);
     float ambient_coefficient = 0.2f;
     float diffuse_coefficient = 0.8f;
     
     vec3 light_vector = normalize(light_position - FragPos);
     vec3 view_vector = normalize(view_position - FragPos);
     vec3 reflect_vector = reflect(-light_vector, Normal);
     
     // diffuse lighting
     float diffuse = max(dot(light_vector, Normal), 0.0);
     vec3 diffuse_light = diffuse * diffuse_coefficient * light_color * diffuse_color;

     // ambient lighting
     vec3 ambient_light = ambient_coefficient * light_color * ambient_color;

     // specular lighting
     float specular = pow(max(dot(reflect_vector, view_vector), 0.0), shininess);
     vec3 specular_light = specular * specular_coefficient * light_color * specular_color;

     vec2 tex0 = (FragPos.xz / 1024) * 128.0f;
     vec4 tex = texture(Texture0, tex0 );

     vec3 bump_normal = 2.0 * texture(Texture1, tex0).rgb - 1.0;
     float lamberFactor= max (dot (light_vector, bump_normal), 0.0) ;
     
     // final color
     FragColor = ((vec4(ambient_light + diffuse_light + specular_light, 1.0) + (tex * 0.3)) );
     //FragColor = vec4(diffuse_color, 1.0) * lamberFactor;
}
