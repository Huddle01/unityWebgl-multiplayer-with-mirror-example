function CreateMediaElementSource(audioContext,audioElement)
{
    const track = new MediaElementAudioSourceNode(audioCtx, {
        mediaElement: audioElement,
      });
    return track;
}