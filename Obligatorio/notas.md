# Apuntes del primer obligatorio

## Manual de usuario

1. Iniciar aplicación Server.
2. Iniciar aplicación Client.
3. Utilizar la aplicación

## Desglose de requerimientos

### Cliente

* RF1 - Conexión y desconexión al servidor.

    El cliente se conecta al servidor de manera automática al iniciarse la aplicación. La desconexión actualmente presenta problemas.

* RF2 - Publicación de juego.
    
    Opción 1 del menú del cliente. Luego de dado de alta al juego se notifica al cliente con un mensaje del lado del servidor. Se puede chequear la adición haciendo uso de la opción 6 del menú (ver todos).
    ACA AGREGAR ACLARACIÓN TEMA RUTAS PARA LA CARÁTULA.

    Por esta iteración, no tenemos muchos chequeos sobre los datos recibidos del lado del servidor, lo cual puede llegar a llevar a problemas con las carátulas(esto es hacer un chequeo boludo pa q no se repita el nombre y chau).

* RF3 - Baja y modificación de juego.

    Opciones X y Z del menú del cliente. Si el juego está en el sistema, modifica sus datos con los recibidos, de lo contrario notifica que el id es inválido (está chequeado esto?).

* RF4 - Búsqueda de juegos.

    Opción 7 del menú del cliente. Se puede buscar por nombre/título del juego, el cual retorna matches parciales. Se puede buscar por categoría, que retorna solo matches absolutos. Se puede buscar por calificación, se retornan juegos con promedio de calificaciones >= al parámetro de búsqueda.

    Importante a la hora de probar búsqueda por calificación, tener en cuenta que los juegos que aún no han sido calificados tienen una calificación nula, es decir, no van a ser tenidos en cuenta a la hora de evaluar los juegos que cumplan con la condición.

* RF5 - Calificación de un juego.

    Opción 2 del menú del cliente. Se permite calificar títulos, luego podemos verificar que la calificación quedó registrada de manera exitosa en el detalle del juego calificado.

* RF6 - Detalle de un juego.

    Opción X del menú del cliente. Se busca por id del juego, el cual se puede obtener utilizando la opción de listar todos. Nos trae toda la información del juego disponible en el servidor, incluida la lista de las calificaciones obtenidas con sus respectivos comentarios.

    La funcionalidad de la descarga de la carátula no se implementa por instrucción de dejarlo para próxima iteración, pero sería relativamente sencillo, replicando de manera inversa el envío realizado del cliente al servidor.

### Servidor

* RF1 - Aceptar pedidos de conexión de un cliente.

    El servidor acepta varias conexiones en paralelo, y se maneja perfectamente respecto al acceso a datos.

* RF2 - Ver catálogo de juegos.

    El servidor permite ver el catálogo de juegos desde cualquier cliente, más allá de que se hayan realizado las adiciones en uno y la lectura en otro.

* RF3 - Adquirir un juego.

    Este requerimiento queda para próxima iteración dado que se decidió dejar directamente el manejo de usuarios para otra iteración.

* RF4 - Publicar un juego.

    Una vez publicado un juego, se puede verificar la creación del mismo desde otro cliente sin problemas.

* RF5 - Publicar una calificación de un juego.

    Al igual que con los juegos, una vez publicada de manera exitosa la calificación se puede verificar desde otro cliente sin problemas.

* RF6 - Buscar juegos.

    Al igual que con el resto de las funcionalidades, dado que podemos ver el catálogo también podemos filtrar el mismo.

* RF7 - Ver detalle de un juego.

    Funciona de manera correcta igual que la anterior. No se permite la descarga de carátula aún pero la adición de esta funcionalidad no debería ser compleja.


## Manejo de paralelismo y concurrencia

Este fue el principal desafío encontrado en este obligatorio. Nos decantamos por la utilización de una clase ServerState que almacenase el estado del servidor durante su ejecución en memoria. Esta clase es estática y aplica el patrón Singleton, al cual le agregamos el uso de una serie de locks para asegurar la integridad de las operaciones sobre la misma. Las operaciones de lectura sobre las listas  de entidades de dominio son de libre acceso, mientras que las de escritura sobre las mismas tienen un lock individual (ej, si el cliente A está escribiendo a la lista de reviews, el cliente B puede al mismo tiempo escribir a la lista de usuarios).

## Protocolo utilizado