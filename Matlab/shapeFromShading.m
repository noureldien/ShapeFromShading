function [Z] = shapeFromShading(lights, imageLocation, imageExtension, imageName, imageCount, imageHeight, imageWidth )
% shape from shading using multible images with their light source

m = imageHeight;
n = imageWidth;
S= lights;

% load images
imgs = zeros(imageCount,m,n);
for i=1:imageCount
    img = imread(strcat(imageLocation, imageName, int2str(i), imageExtension));
    imgs(i,:,:) = rgb2gray(img);
end

% normalize lights
for i=1:size(lights,1)
    light = lights(i,:);
    lights(i,:) = light/norm(light);
end

b=double(ones(m,n,3));
p=double(ones(m,n));
q=p;
Z=p;

for i=1:m
    for j=1:n
        E=imgs(:,i,j);
        tb= (inv(S'*S))*S'*E;
        nbm = norm(tb);
        
        if( nbm == 0)
            b(i,j,:) = 0;
        else
            b(i,j,:) = tb / nbm;
        end
        
        tM = [b(i,j,1) b(i,j,2) b(i,j,3)];
        nbm = norm(tM);
        if( nbm == 0)
            tM = [0 0 0];
        else
            tM = tM / nbm;
        end
        
        p(i,j)=tM(1,1);
        q(i,j)=tM(1,2);
    end
end

for i=1:m
    for j=1:n
        Z(i,j) = sum(q(1:i, 1)) + sum(p(i,1:j));
    end
end
Z = Z*-1;

end




