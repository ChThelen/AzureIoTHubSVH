package de.qaware.pn.service;

import com.microsoft.azure.sdk.iot.service.*;
import com.microsoft.azure.sdk.iot.service.devicetwin.DeviceMethod;
import com.microsoft.azure.sdk.iot.service.devicetwin.MethodResult;
import com.microsoft.azure.sdk.iot.service.exceptions.IotHubException;
import com.microsoft.azure.storage.StorageException;
import com.microsoft.azure.storage.blob.CloudBlobContainer;
import com.microsoft.azure.storage.blob.SharedAccessBlobPermissions;
import com.microsoft.azure.storage.blob.SharedAccessBlobPolicy;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.time.Duration;
import java.time.Instant;
import java.util.Date;
import java.util.EnumSet;
import java.util.concurrent.TimeUnit;

@Component
public class IotHubConnection {
    public static final String iotHubConnectionString = "HostName=SVH-Hub.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=svDdlDs6wb5x3upGXekO16sQYiY+1TpjaZvTYn/Pcoo=";
    public static final Long responseTimeout = TimeUnit.SECONDS.toSeconds(200);
    public static final Long connectTimeout = TimeUnit.SECONDS.toSeconds(5);

    private final ServiceClient serviceClient;

    public IotHubConnection() throws IOException, IotHubException, InterruptedException {
        serviceClient = ServiceClient.createFromConnectionString(
                iotHubConnectionString, IotHubServiceClientProtocol.AMQPS);
    }

    public void sendMessageToDevice(String deviceId) throws IOException, IotHubException, InterruptedException {
        if (serviceClient != null) {
            serviceClient.open();
            FeedbackReceiver feedbackReceiver = serviceClient
                    .getFeedbackReceiver(deviceId);
            if (feedbackReceiver != null) feedbackReceiver.open();

            Message messageToSend = new Message("Cloud to device message.");
            messageToSend.setDeliveryAcknowledgement(DeliveryAcknowledgement.Full);

            serviceClient.send(deviceId, messageToSend);
            System.out.println("Message sent to device");

            FeedbackBatch feedbackBatch = feedbackReceiver.receive(10000);
            if (feedbackBatch != null) {
                System.out.println("Message feedback received, feedback time: "
                        + feedbackBatch.getEnqueuedTimeUtc().toString());
            }

            if (feedbackReceiver != null) feedbackReceiver.close();
            serviceClient.close();
        }
    }

    public String getContainerSasUri(CloudBlobContainer container) throws InvalidKeyException, StorageException {
        //Set the expiry time and permissions for the container.
        //In this case no start time is specified, so the shared access signature becomes valid immediately.
        SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
        Date expirationDate = Date.from(Instant.now().plus(Duration.ofDays(1)));
        sasConstraints.setSharedAccessExpiryTime(expirationDate);
        EnumSet<SharedAccessBlobPermissions> permissions = EnumSet.of(
                SharedAccessBlobPermissions.WRITE,
                SharedAccessBlobPermissions.LIST,
                SharedAccessBlobPermissions.READ,
                SharedAccessBlobPermissions.DELETE);
        sasConstraints.setPermissions(permissions);

        //Generate the shared access signature on the container, setting the constraints directly on the signature.
        String sasContainerToken = container.generateSharedAccessSignature(sasConstraints, null);

        //Return the URI string for the container, including the SAS token.
        return container.getUri() + "?" + sasContainerToken;
    }

    public Object callMethod(String deviceId, String methodName, String payload) throws IOException {
        System.out.println("Starting sample...");
        DeviceMethod methodClient = DeviceMethod.createFromConnectionString(iotHubConnectionString);

        try
        {
            // Manage complete Method
            System.out.println("Getting device Method");
            MethodResult result = methodClient.invoke(deviceId, methodName, responseTimeout, connectTimeout, payload);
            if(result == null)
            {
                throw new IOException("Method invoke returns null");
            }
            System.out.println("Status=" + result.getStatus());
            System.out.println("Payload=" + result.getPayload());
            return result.getPayload();
        }
        catch (IotHubException e)
        {
            System.out.println(e.getMessage());
        }
        return null;
    }

    public Device registerDevice(String externalDeviceId) throws IOException, NoSuchAlgorithmException {
        RegistryManager registryManager = RegistryManager.createFromConnectionString(iotHubConnectionString);

        Device device = Device.createFromId(externalDeviceId, null, null);
        try {
            device = registryManager.addDevice(device);
        } catch (IotHubException iote) {
            try {
                device = registryManager.getDevice(externalDeviceId);
            } catch (IotHubException iotf) {
                iotf.printStackTrace();
            }
        }
        System.out.println("Device ID: " + device.getDeviceId());
        System.out.println("Device key: " + device.getPrimaryKey());

        return device;
    }
}
