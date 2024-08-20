# unityWebgl-multiplayer-with-mirror-example

# Huddle01 Plugin Integration with Mirror SDK in Unity

This project demonstrates how to integrate the Huddle01 Unity WebGL plugin with the Mirror SDK for multiplayer projects.

## Assets Used

- **Mirror SDK:** [Mirror Networking Documentation](https://mirror-networking.gitbook.io/docs)
- **Invector 3rd Person Controller LITE:** [Invector Basic Locomotion](https://assetstore.unity.com/packages/tools/game-toolkits/third-person-controller-basic-locomotion-free-82048)

## Server Build Instructions

1. **Switch Platform:** Change the platform to Dedicated Server.
2. **Open Scene:** Open the scene `Invector_BasicLocomotion`.
3. **Disable Components:**
   - Disable the GameObject `Manager`.
   - Disable the `MetaverseHuddleCommManager` component attached to the `Manager` GameObject.
4. **Provide Values in `Constant.cs`:**
   - `ProjectId`
   - `ApiKey`
   - `RoomId`
5. **Implement Token Method:** Implement the `CmdSetHuddleToken` method to get a token from the server. Refer to the [Huddle01 API Documentation](https://docs.huddle01.com/docs/apis/join-room-token) for implementation.
6. **Build:** Proceed with the build process.

## Client Build Instructions

1. **Switch Platform:** Change the platform to WebGL.
2. **Open Scene:** Open the scene `Invector_BasicLocomotion`.
3. **Enable Components:**
   - Enable the GameObject `Manager`.
   - Enable the `MetaverseHuddleCommManager` component attached to the `Manager` GameObject.
4. **Build:** Proceed with the build process.

For detailed build instructions, refer to the [Mirror Networking Documentation](https://mirror-networking.gitbook.io/docs/manual/transports/websockets-transport).

