// Returns the rendered image bounds in pixels relative to its nearest positioned parent.
// Accounts for both the element's position within the container (flex centering, etc.)
// and any object-fit: contain letterboxing within the element itself.
window.getImageRenderBounds = function (imgElement) {
    if (!imgElement) return null;

    const naturalWidth  = imgElement.naturalWidth;
    const naturalHeight = imgElement.naturalHeight;

    const container     = imgElement.parentElement;
    const containerRect = container.getBoundingClientRect();
    const imgRect       = imgElement.getBoundingClientRect();

    if (naturalWidth === 0 || naturalHeight === 0) {
        return { left: 0, top: 0, width: containerRect.width, height: containerRect.height };
    }

    // Offset of the img element from the SVG container's top-left
    const offsetLeft = imgRect.left - containerRect.left;
    const offsetTop  = imgRect.top  - containerRect.top;

    // object-fit: contain letterboxing within the img element itself
    const elemWidth  = imgRect.width;
    const elemHeight = imgRect.height;
    const elemRatio  = elemWidth / elemHeight;
    const imageRatio = naturalWidth / naturalHeight;

    let renderedWidth, renderedHeight, innerLeft, innerTop;
    if (imageRatio > elemRatio) {
        renderedWidth  = elemWidth;
        renderedHeight = elemWidth / imageRatio;
        innerLeft = 0;
        innerTop  = (elemHeight - renderedHeight) / 2;
    } else {
        renderedHeight = elemHeight;
        renderedWidth  = elemHeight * imageRatio;
        innerLeft = (elemWidth - renderedWidth) / 2;
        innerTop  = 0;
    }

    return {
        left:   offsetLeft + innerLeft,
        top:    offsetTop  + innerTop,
        width:  renderedWidth,
        height: renderedHeight
    };
};
