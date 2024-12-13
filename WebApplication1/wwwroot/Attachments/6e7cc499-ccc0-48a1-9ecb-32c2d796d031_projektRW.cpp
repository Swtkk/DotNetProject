#include <glad/gl.h>
#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
#include "linmath.h"
#include <stdlib.h>
#include <stdio.h>
#include <math.h>

#define MAX_CUBES 10

typedef struct {
    vec3 position;
} Cube;

typedef struct {
    vec3 position;
    vec3 rotation;
} Camera;

static Cube cubes[MAX_CUBES];
static Camera camera = { {0.f, 0.f, 3.f}, {0.f, 0.f, 0.f} };
static float fov = 45.0f;

static const struct {
    float x, y, z;
    float r, g, b;
} cube_vertices[] = {
    -0.5f, -0.5f, -0.5f, 1.f, 0.f, 0.f,  // lewy dolny
     0.5f, -0.5f, -0.5f, 1.f, 0.f, 0.f,  // prawy dolny
     0.5f,  0.5f, -0.5f, 1.f, 0.f, 0.f,  // prawy górny
    -0.5f,  0.5f, -0.5f, 1.f, 0.f, 0.f,  // lewy górny

    // Ściana tylna (zielony)
    -0.5f, -0.5f,  0.5f, 0.f, 1.f, 0.f,  // lewy dolny
     0.5f, -0.5f,  0.5f, 0.f, 1.f, 0.f,  // prawy dolny
     0.5f,  0.5f,  0.5f, 0.f, 1.f, 0.f,  // prawy górny
    -0.5f,  0.5f,  0.5f, 0.f, 1.f, 0.f,  // lewy górny

    // Ściana lewa (niebieski)
    -0.5f, -0.5f, -0.5f, 0.f, 0.f, 1.f,  // lewy dolny przedni
    -0.5f,  0.5f, -0.5f, 0.f, 0.f, 1.f,  // lewy górny przedni
    -0.5f,  0.5f,  0.5f, 0.f, 0.f, 1.f,  // lewy górny tylny
    -0.5f, -0.5f,  0.5f, 0.f, 0.f, 1.f,  // lewy dolny tylny

    // Ściana prawa (żółty)
     0.5f, -0.5f, -0.5f, 1.f, 1.f, 0.f,  // prawy dolny przedni
     0.5f,  0.5f, -0.5f, 1.f, 1.f, 0.f,  // prawy górny przedni
     0.5f,  0.5f,  0.5f, 1.f, 1.f, 0.f,  // prawy górny tylny
     0.5f, -0.5f,  0.5f, 1.f, 1.f, 0.f,  // prawy dolny tylny

     // Ściana dolna (magenta)
     -0.5f, -0.5f, -0.5f, 1.f, 0.f, 1.f,  // lewy dolny przedni
      0.5f, -0.5f, -0.5f, 1.f, 0.f, 1.f,  // prawy dolny przedni
      0.5f, -0.5f,  0.5f, 1.f, 0.f, 1.f,  // prawy dolny tylny
     -0.5f, -0.5f,  0.5f, 1.f, 0.f, 1.f,  // lewy dolny tylny

     // Ściana górna (biały)
     -0.5f,  0.5f, -0.5f, 1.f, 1.f, 1.f,  // lewy górny przedni
      0.5f,  0.5f, -0.5f, 1.f, 1.f, 1.f,  // prawy górny przedni
      0.5f,  0.5f,  0.5f, 1.f, 1.f, 1.f,  // prawy górny tylny
     -0.5f,  0.5f,  0.5f, 1.f, 1.f, 1.f   // lewy górny tylny
};

static const unsigned int cube_indices[] = {
    0, 1, 2, 2, 3, 0, // Front
    4, 5, 6, 6, 7, 4, // Back
    0, 1, 5, 5, 4, 0, // Bottom
    3, 2, 6, 6, 7, 3, // Top
    0, 3, 7, 7, 4, 0, // Left
    1, 2, 6, 6, 5, 1  // Right
};

// Shaders
static const char* vertex_shader_text =
"#version 110\n"
"uniform mat4 MVP;\n"
"attribute vec3 vPos;\n"
"attribute vec3 vCol;\n"
"varying vec3 color;\n"
"void main()\n"
"{\n"
"    gl_Position = MVP * vec4(vPos, 1.0);\n"
"    color = vCol;\n"
"}\n";

static const char* fragment_shader_text =
"#version 110\n"
"varying vec3 color;\n"
"void main()\n"
"{\n"
"    gl_FragColor = vec4(color, 1.0);\n"
"}\n";

void generate_cubes() {
    for (int i = 0; i < MAX_CUBES; i++) {
        cubes[i].position[0] = (rand() % 5 - 2); // X: od -2 do 2
        cubes[i].position[1] = (rand() % 5 - 2); // Y: od -2 do 2
        cubes[i].position[2] = -(rand() % 5 + 1); // Z: od -1 do -5
    }
}

void key_callback(GLFWwindow* window, int key, int scancode, int action, int mods) {
    float speed = 0.1f;

    // Kierunek kamery w uk�adzie �wiatowym
    vec3 forward = {
        sinf(camera.rotation[1] * (3.14159f / 180.0f)),
        0,
        -cosf(camera.rotation[1] * (3.14159f / 180.0f))
    };

    vec3 right = {
        cosf(camera.rotation[1] * (3.14159f / 180.0f)),
        0,
        sinf(camera.rotation[1] * (3.14159f / 180.0f))
    };

    if (action == GLFW_PRESS || action == GLFW_REPEAT) {
        if (key == GLFW_KEY_W) {
            camera.position[0] += forward[0] * speed; // Ruch do przodu
            camera.position[2] += forward[2] * speed;
        }
        if (key == GLFW_KEY_S) {
            camera.position[0] -= forward[0] * speed; // Ruch do ty�u
            camera.position[2] -= forward[2] * speed;
        }
        if (key == GLFW_KEY_A) {
            camera.position[0] -= right[0] * speed; // Ruch w lewo
            camera.position[2] -= right[2] * speed;
        }
        if (key == GLFW_KEY_D) {
            camera.position[0] += right[0] * speed; // Ruch w prawo
            camera.position[2] += right[2] * speed;
        }
        if (key == GLFW_KEY_KP_ADD && fov < 120.f) fov += 1.f; // Zwi�ksz FOV
        if (key == GLFW_KEY_KP_SUBTRACT && fov > 10.f) fov -= 1.f; // Zmniejsz FOV
    }
}

void cursor_position_callback(GLFWwindow* window, double xpos, double ypos) {
    static double last_x = 320, last_y = 240;
    double x_offset = xpos - last_x;
    double y_offset = ypos - last_y; // Inverted Y-axis
    last_x = xpos;
    last_y = ypos;

    camera.rotation[0] += y_offset * 0.1f;
    camera.rotation[1] += x_offset * 0.1f;
}

void check_shader_compile_status(GLuint shader, const char* shader_type) {
    GLint status;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
    if (status == GL_FALSE) {
        char buffer[512];
        glGetShaderInfoLog(shader, 512, NULL, buffer);
        fprintf(stderr, "%s Shader Compile Error: %s\n", shader_type, buffer);
    }
}
void process_input(GLFWwindow* window) {
    float speed = 0.03f;

    // Kierunek kamery w układzie świata
    vec3 forward = {
        sinf(camera.rotation[1] * (3.14159f / 180.0f)),
        0,
        -cosf(camera.rotation[1] * (3.14159f / 180.0f))
    };

    vec3 right = {
        cosf(camera.rotation[1] * (3.14159f / 180.0f)),
        0,
        sinf(camera.rotation[1] * (3.14159f / 180.0f))
    };

    if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS) {
        camera.position[0] += forward[0] * speed; // Ruch do przodu
        camera.position[2] += forward[2] * speed;
    }
    if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS) {
        camera.position[0] -= forward[0] * speed; // Ruch do tyłu
        camera.position[2] -= forward[2] * speed;
    }
    if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS) {
        camera.position[0] -= right[0] * speed; // Ruch w lewo
        camera.position[2] -= right[2] * speed;
    }
    if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS) {
        camera.position[0] += right[0] * speed; // Ruch w prawo
        camera.position[2] += right[2] * speed;
    }
    if (glfwGetKey(window, GLFW_KEY_KP_ADD) == GLFW_PRESS && fov < 120.f) fov += .2f; // Zwiększ FOV
    if (glfwGetKey(window, GLFW_KEY_KP_SUBTRACT) == GLFW_PRESS && fov > 10.f) fov -= .2f; // Zmniejsz FOV
}

int main(void) {
    GLFWwindow* window;

    if (!glfwInit()) return -1;

    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);

    window = glfwCreateWindow(800, 600, "First Person Camera", NULL, NULL);
    if (!window) {
        glfwTerminate();
        return -1;
    }

    glfwMakeContextCurrent(window);
    gladLoadGL(glfwGetProcAddress);
    glfwSetKeyCallback(window, key_callback);
    glfwSetCursorPosCallback(window, cursor_position_callback);
    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

    generate_cubes();

    GLuint vao, vbo, ebo;
    glGenVertexArrays(1, &vao);
    glBindVertexArray(vao);

    glGenBuffers(1, &vbo);
    glBindBuffer(GL_ARRAY_BUFFER, vbo);
    glBufferData(GL_ARRAY_BUFFER, sizeof(cube_vertices), cube_vertices, GL_STATIC_DRAW);

    glGenBuffers(1, &ebo);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(cube_indices), cube_indices, GL_STATIC_DRAW);

    GLuint vertex_shader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(vertex_shader, 1, &vertex_shader_text, NULL);
    glCompileShader(vertex_shader);
    check_shader_compile_status(vertex_shader, "Vertex");

    GLuint fragment_shader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fragment_shader, 1, &fragment_shader_text, NULL);
    glCompileShader(fragment_shader);
    check_shader_compile_status(fragment_shader, "Fragment");

    GLuint program = glCreateProgram();
    glAttachShader(program, vertex_shader);
    glAttachShader(program, fragment_shader);
    glLinkProgram(program);
    glUseProgram(program);

    GLint mvp_location = glGetUniformLocation(program, "MVP");
    GLint vpos_location = glGetAttribLocation(program, "vPos");
    GLint vcol_location = glGetAttribLocation(program, "vCol");

    glVertexAttribPointer(vpos_location, 3, GL_FLOAT, GL_FALSE, sizeof(cube_vertices[0]), (void*)0);
    glEnableVertexAttribArray(vpos_location);

    glVertexAttribPointer(vcol_location, 3, GL_FLOAT, GL_FALSE, sizeof(cube_vertices[0]), (void*)(3 * sizeof(float)));
    glEnableVertexAttribArray(vcol_location);

    glEnable(GL_DEPTH_TEST);
    glClearColor(0.1f, 0.1f, 0.2f, 1.0f);

    while (!glfwWindowShouldClose(window)) {
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        process_input(window); // Sprawdź wciśnięte klawisze

        mat4x4 projection, view, model, mvp;
        mat4x4_perspective(projection, fov * (3.14159f / 180.f), 800.f / 600.f, 0.1f, 100.f);

        mat4x4_identity(view);
        mat4x4_rotate_X(view, view, camera.rotation[0] * (3.14159f / 180.f));
        mat4x4_rotate_Y(view, view, camera.rotation[1] * (3.14159f / 180.f));
        mat4x4_translate_in_place(view, -camera.position[0], -camera.position[1], -camera.position[2]);

        for (int i = 0; i < MAX_CUBES; i++) {
            mat4x4_identity(model);
            mat4x4_translate(model, cubes[i].position[0], cubes[i].position[1], cubes[i].position[2]);
            mat4x4_mul(mvp, projection, view);
            mat4x4_mul(mvp, mvp, model);
            glUniformMatrix4fv(mvp_location, 1, GL_FALSE, (const GLfloat*)mvp);
            glDrawElements(GL_TRIANGLES, sizeof(cube_indices) / sizeof(unsigned int), GL_UNSIGNED_INT, 0);
        }

        glfwSwapBuffers(window);
        glfwPollEvents();
    }


    glfwTerminate();
    return 0;
}
