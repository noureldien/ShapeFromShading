function depth = variationalApproach( image, iternations, lambda )
% shape from shading using variational approach,
% lack-of-smoothness regularization term by Horn & Ikeuchi

sz = size(image);
Z = image / max(max(image));

Zx = 0.5 * imfilter(Z, [-1 0 1]);
Zy = 0.5 * imfilter(Z, [-1; 0; 1]);

gt = zeros(sz);
ft = zeros(sz);

ft(Zx >= 0.5 * max(max(Zx))) = -2;
ft(Zx < 0.5 * min(min(Zx))) = 2;
gt(Zy >= 0.5 * max(max(Zy))) = 2;
gt(Zy < 0.5 * min(min(Zy))) = -2;

y = ft.^2 + gt.^2;

fs = ft;
gs = gt;

fs(y~=4) = 0;
gs(y~=4) = 0;

kernel = (1/8) * [1 1 1; 1 0 1; 1 1 1];
for i = 1 : iternations
    s = fs.^2 + gs.^2;
    depth = (-s  + 4) ./ (s + 4);
    rnorm = 1./(s + 4 ).^2;
    fmid = imfilter(fs, kernel);
    diff = double(Z) - depth;
    f = (1/lambda) * fs.* rnorm .* diff;
    % fs = fsmooth + fdata;
    s = ft==0;
    fs(s) = fmid(s) - f(s);
    gmid = imfilter(gs, kernel);
    g = (1/lambda) * gs .* rnorm .* diff;
    % gs = gsmooth + gdata;
    s = gt==0;
    gs(s) = gmid(s) - g(s);
end

end
